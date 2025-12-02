// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCFRPendingWordContentCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassForumDetailResultId { get; set; }

        public string? WordContent { get; set; }

        public IList<string>? FilePaths { get; set; }
    }

    public class UpdateCFRPendingWordContentCommandHandler : IRequestHandler<UpdateCFRPendingWordContentCommand, MethodResult<bool>>
    {
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly SubmitClassForumGradingPublisher _submitClassForumGradingPublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ISystemService _systemService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly SetTimeClassForumDonePublisher _setTimeClassForumDonePublisher;

        public UpdateCFRPendingWordContentCommandHandler(IClassForumDetailResultRepository classForumDetailResultRepository,
                                                         IClassForumResultRepository classForumResultRepository,
                                                         IClassForumRepository classForumRepository,
                                                         ICourseRepository courseRepository,
                                                         SubmitClassForumGradingPublisher submitClassForumGradingPublisher,
                                                         QuestBoardPublisher questBoardPublisher,
                                                         ISystemService systemService,
                                                         CreateTokenHistoryPublisher createTokenHistoryPublisher,
                                                         ICourseResultRepository courseResultRepository,
                                                         SetTimeClassForumDonePublisher setTimeClassForumDonePublisher)
        {
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _courseRepository = courseRepository;
            _submitClassForumGradingPublisher = submitClassForumGradingPublisher;
            _questBoardPublisher = questBoardPublisher;
            _systemService = systemService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _courseResultRepository = courseResultRepository;
            _setTimeClassForumDonePublisher = setTimeClassForumDonePublisher;
        }

        public async Task<MethodResult<bool>> Handle(UpdateCFRPendingWordContentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            bool isFirst = false;

            if (StringHelper.IsBase64Image(request.WordContent))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.Base64InText));
                return methodResult;
            }

            // Check từ khoá cấm
            //var listForbiddenWordResultWordContent = await _systemService.CheckContainForbiddenWord(request.WordContent ?? string.Empty);
            //var containsForbiddenWord = (listForbiddenWordResultWordContent.Content?.Result ?? Enumerable.Empty<string>()).Distinct().ToList();
            //if (containsForbiddenWord.Any())
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords), string.Join(", ", containsForbiddenWord));
            //    return methodResult;
            //}

            var classForumDetailResult = await _classForumDetailResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);
            if (classForumDetailResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumDetailResult));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.LessonResult)
                                                                    .FirstOrDefaultAsync(x => x.Id == classForumDetailResult.ClassForumResultId, cancellationToken);
            if (classForumResult == null || classForumResult.LessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(classForumResult.LessonResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == classForumResult.LessonResult.LessonId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            if (classForumDetailResult.Status != EnumClassForumResultStatus.PendingSpeechToText)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.RecordNotStatusPendingSpeechToText), nameof(classForumDetailResult.Status));
                return methodResult;
            }

            if (classForumDetailResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
            {
                isFirst = true;
            }

            await _classForumDetailResultRepository.ExecuteTransactionAsync(async () =>
            {
                classForumDetailResult.WordContent = request.WordContent;
                classForumDetailResult.ProcessDate = isFirst ? DateTime.UtcNow : null;
                classForumDetailResult.MediaType = MediaHelper.GetMediaType(classForumDetailResult.ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault());
                classForumDetailResult.Status = string.IsNullOrEmpty(request.WordContent) ? EnumClassForumResultStatus.ErrorSpeechToText : EnumClassForumResultStatus.Pending;

                await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.ClassForumResultId, c.SubmissionCount };
                });

                var token = isFirst ? await GetTokenAsync(classForum, classForumResult, course.CourseType) : null;
                await UpdateClassForumResult(classForumResult, token, cancellationToken);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            // đẩy lên AI chấm điểm
            await PublishAIClassForumResponseAsync(classForumDetailResult, classForum, request.WordContent ?? string.Empty, cancellationToken);

            // cập nhật nhiệm vụ
            await DoQuestBoard(classForumResult.StudentId, EnumQuestBoardType.BeginnerQuests, EnumQuestBoardCategory.CompleteTheFirstClassForum, cancellationToken);
            await DoQuestBoard(classForumResult.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.SharedRocketLaunch, cancellationToken);

            if (isFirst)
            {
                // set job
                await _setTimeClassForumDonePublisher.Publish(new Core.Base.BaseModels.BaseQueueModel { QueueId = classForumResult.Id.ToString() }, cancellationToken);

                // cộng coin
                await PublishCreateTokenHistory(classForumResult, classForum, course, classForumResult.StudentId, classForumResult.CreatedUserId, cancellationToken);
            }

            return methodResult;
        }

        private async Task UpdateClassForumResult(ClassForumResult classForumResult, int? token, CancellationToken cancellationToken)
        {
            classForumResult.TokenFirstTime = token;
            classForumResult.IsPendingSpeechToText = false;
            await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
            });
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardType type, EnumQuestBoardCategory category, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = type,
                Category = category,
                Value = 1
            }, cancellationToken);
        }

        private static EnumTokenMission GetTokenMission(ClassForum classForum, ClassForumResult classForumResult)
        {
            return classForum.CourseSkill == EnumCourseSkill.Writing ? EnumTokenMission.ClassForumWriting
               : classForumResult.MediaType == EnumMediaType.Video ? EnumTokenMission.ClassForumSpeakingVideo
               : EnumTokenMission.ClassForumSpeakingAudio;
        }

        private async Task<int?> GetTokenAsync(ClassForum classForum, ClassForumResult classForumResult, EnumCourseType courseType)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.Learn,
                Mission = GetTokenMission(classForum, classForumResult),
                CourseType = courseType
            });

            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }

            var tokenConfig = tokenConfigs.Content?.Result;
            return (int?)(tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default);
        }

        private async Task PublishCreateTokenHistory(ClassForumResult classForumResult, ClassForum classForum, Course course, Guid studentId, Guid userId, CancellationToken cancellationToken)
        {
            var tokenHistorys = new List<TokenHistoryQueueModel>
            {
              new TokenHistoryQueueModel
              {
                  ObjectId = classForumResult.Id,
                  VolatileToken = classForumResult.TokenFirstTime.HasValue ? classForumResult.TokenFirstTime.Value : default,
                  Feature = EnumTokenFeature.Learn,
                  CourseResultId = _courseResultRepository.Queryable.FirstOrDefault(x => x.CourseId == course.Id && x.StudentId == studentId)?.Id,
                  Mission = GetTokenMission(classForum,classForumResult),
                  Type = EnumTokenHistoryType.Recevived,
                  UserId = userId,
              }
            };

            await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken);
        }

        private async Task PublishAIClassForumResponseAsync(ClassForumDetailResult classForumDetailResult, ClassForum classForum, string wordContent, CancellationToken cancellationToken)
        {
            if (classForum.IsAlFeedBack)
            {
                await _submitClassForumGradingPublisher.Publish(new ClassForumAIResponseModelV2
                {
                    ClassForumResultId = classForumDetailResult.ClassForumResultId,
                    ClassForumDetailResultId = classForumDetailResult.Id,
                    WordContent = wordContent,
                    SubmissionCount = classForumDetailResult.SubmissionCount ?? default
                }, cancellationToken);
            }
        }
    }
}
