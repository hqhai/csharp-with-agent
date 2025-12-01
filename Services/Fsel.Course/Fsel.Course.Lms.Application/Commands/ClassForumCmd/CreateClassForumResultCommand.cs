// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class CreateClassForumResultCommand : CreateClassForumResultCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class CreateClassForumResultCommandHandler : IRequestHandler<CreateClassForumResultCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILogger<CreateClassForumResultCommand> _logger;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly SubmitClassForumGradingPublisher _submitClassForumGradingPublisher;
        private readonly ISystemService _systemService;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly SetTimeClassForumDonePublisher _setTimeClassForumDonePublisher;
        private readonly IHostEnvironment _environment;
        private readonly IClassForumResultFileRepository _classForumResultFileRepository;
        private readonly ClassForumPronunciationPublisher _classForumPronunciationPublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public const int DisplayOrderFirst = 0;
        public const int DisplayOrderSecond = 1;

        public CreateClassForumResultCommandHandler(IMapper mapper,
            ICourseResultRepository courseResultRepository,
            ILogger<CreateClassForumResultCommand> logger,
            CreateTokenHistoryPublisher createTokenHistoryPublisher, ICourseRepository courseRepository, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository, SubmitClassForumGradingPublisher submitClassForumGradingPublisher, ISystemService systemService, IClassForumDetailResultRepository classForumDetailResultRepository, SetTimeClassForumDonePublisher setTimeClassForumDonePublisher, QuestBoardPublisher questBoardPublisher, IHostEnvironment environment, ClassForumPronunciationPublisher classForumPronunciationPublisher, IClassForumResultFileRepository classForumResultFileRepository)
        {
            _mapper = mapper;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _courseResultRepository = courseResultRepository;
            _logger = logger;
            _courseRepository = courseRepository;
            _authContext = authContext;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _submitClassForumGradingPublisher = submitClassForumGradingPublisher;
            _systemService = systemService;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _setTimeClassForumDonePublisher = setTimeClassForumDonePublisher;
            _questBoardPublisher = questBoardPublisher;
            _environment = environment;
            _classForumPronunciationPublisher = classForumPronunciationPublisher;
            _classForumResultFileRepository = classForumResultFileRepository;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumResultModel> methodResult = new MethodResult<ClassForumResultModel>();
            if (StringHelper.IsBase64Image(request.Content) || StringHelper.IsBase64Image(request.WordContent))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.Base64InText));
                return methodResult;
            }
            //// Check từ khoá cấm
            //var listForbiddenWordResultContent = await _systemService.CheckContainForbiddenWord(request.Content ?? string.Empty);
            //var listForbiddenWordResultWordContent = await _systemService.CheckContainForbiddenWord(request.WordContent ?? string.Empty);
            //var containsForbiddenWord = (listForbiddenWordResultContent.Content?.Result ?? Enumerable.Empty<string>())
            // .Concat(listForbiddenWordResultWordContent.Content?.Result ?? Enumerable.Empty<string>()).Distinct()
            // .ToList();

            //if (containsForbiddenWord.Any())
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords), string.Join(", ", containsForbiddenWord));
            //    return methodResult;
            //}

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(lessonResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == lessonResult.LessonId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable
                    .Include(x => x.ClassForumDetailResults.OrderBy(x => x.CreatedDate))
                    .ThenInclude(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumScores)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);

            var query = _classForumDetailResultRepository.Queryable.Include(x => x.ClassForumResultFiles);

            var classForumDetailResultAttemp1 = await query.Where(x => x.SubmissionCount == EnumSubmissionCount.FirstSubmit && classForumResult != null && x.ClassForumResultId == classForumResult.Id).FirstOrDefaultAsync(cancellationToken);
            DateTime? processDateResultAttemp1 = classForumDetailResultAttemp1 != null && classForumDetailResultAttemp1.ProcessDate.HasValue ?
                                                (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing)) ?
                                                classForumDetailResultAttemp1.ProcessDate.Value.AddMinutes(10) :
                                                classForumDetailResultAttemp1.ProcessDate.Value.AddHours(2) :
                                                classForumDetailResultAttemp1?.ProcessDate;

            if (classForumDetailResultAttemp1 != null && processDateResultAttemp1.HasValue && processDateResultAttemp1.Value <= DateTime.UtcNow)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.TimeUpPostClassForum), nameof(classForum));
                return methodResult;
            }
            if (classForumResult != null && classForumResult.ClassForumDetailResults.Where(x => x.Status == EnumClassForumResultStatus.Pending).Count() >= 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumDetailHaveMoreThan2));
                return methodResult;
            }

            ClassForumDetailResult? classForumDetailResult = default;

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (classForumResult == null)
                {
                    classForumResult = await CreateClassForumResultAsync(request, classForum, course, studentId, cancellationToken);
                    var method = await CreateClassForumDetailResultAsync(request, classForum, classForumResult, EnumSubmissionCount.FirstSubmit, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    (classForumResult, classForumDetailResult) = method.Result;
                }
                else if (classForumDetailResultAttemp1 != null && classForumDetailResultAttemp1.ProcessDate.HasValue && classForumResult.ClassForumDetailResults.All(x => x.SubmissionCount != EnumSubmissionCount.SecondSubmit))
                {
                    var method = await CreateClassForumDetailResultAsync(request, classForum, classForumResult, EnumSubmissionCount.SecondSubmit, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    (classForumResult, classForumDetailResult) = method.Result;
                }
                else
                {
                    var method = await UpdateClassForumResultToAttpAsync(request, classForum, course, classForumResult, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    (classForumResult, classForumDetailResult) = method.Result;
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            if (classForumDetailResult != null && request.IsSubmit)
            {
                await PublishAIClassForumResponseAsync(classForumDetailResult, classForum, request, cancellationToken);

                // chấm Pronunciation
                if (classForum.CourseSkill == EnumCourseSkill.Speaking)
                {
                    await _classForumPronunciationPublisher.Publish(new ClassForumPronunciationConsumerModel { ClassForumDetailResultId = classForumDetailResult.Id }, cancellationToken);
                }
            }

            var classForumDetailResults = await _classForumDetailResultRepository.Queryable.Where(x => classForumResult != null && x.ClassForumResultId == classForumResult.Id).ToListAsync(cancellationToken);
            if (classForumResult != null && classForumDetailResults.Any(x => x.Status == EnumClassForumResultStatus.Pending) && classForumDetailResults.Count == (int)EnumSubmissionCount.FirstSubmit)
            {
                var tokenHistorys = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = classForumResult.Id,
                        VolatileToken = classForumResult.TokenFirstTime.HasValue ? classForumResult.TokenFirstTime.Value : default,
                        Feature = EnumTokenFeature.Learn,
                        CourseResultId = _courseResultRepository.Queryable.FirstOrDefault(x=>x.CourseId == course.Id && x.StudentId == studentId)?.Id,
                        Mission = GetTokenMission(classForum,classForumResult),
                        Type = EnumTokenHistoryType.Recevived,
                        UserId = student.UserId,
                    }
                };
                await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken);
            }

            #region Do QuestBoard

            await DoQuestBoard(studentId, EnumQuestBoardType.BeginnerQuests, EnumQuestBoardCategory.CompleteTheFirstClassForum, cancellationToken);
            await DoQuestBoard(studentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.SharedRocketLaunch, cancellationToken);

            #endregion Do QuestBoard

            return methodResult;
        }

        #region Save ClassForum Result

        private async Task<MethodResult<ClassForumDetailResult?>> UpdateClassForumDetailResultAsync(CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            MethodResult<ClassForumDetailResult?> methodResult = new MethodResult<ClassForumDetailResult?>();
            if (!request.ClassForumDetailResultId.HasValue)
            {
                return methodResult;
            }
            var classForumDetailResult = await _classForumDetailResultRepository.GetByIdAsync(request.ClassForumDetailResultId.Value);
            if (classForumDetailResult == null)
            {
                return methodResult;
            }
            _mapper.Map(request, classForumDetailResult);
            classForumDetailResult.Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft;
            classForumDetailResult.ProcessDate = request.IsSubmit && classForumDetailResult.SubmissionCount == EnumSubmissionCount.FirstSubmit ? DateTime.UtcNow : null;
            classForumDetailResult.ClassForumResultFiles = request.FilePaths?.Select(x => new ClassForumResultFile
            {
                FilePath = x,
            }).ToList() ?? new List<ClassForumResultFile>();
            if (!classForumDetailResult.IsValid())
            {
                methodResult.AddErrorBadRequest(classForumDetailResult.ErrorMessages);
                return methodResult;
            }

            await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ClassForumResultId, c.SubmissionCount };
            });
            var classForumResultFileNews = classForumDetailResult.ClassForumResultFiles;

            var classForumResultFiles = await _classForumResultFileRepository.Queryable.Where(x => x.ClassForumDetailResultId == classForumDetailResult.Id).ToListAsync(cancellationToken);
            if (classForumResultFiles.Any())
            {
                await _classForumResultFileRepository.DeleteListAsync(classForumResultFiles);
                await _classForumResultFileRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (classForumResultFileNews.Any())
            {
                foreach (var file in classForumResultFileNews)
                {
                    file.ClassForumDetailResultId = classForumDetailResult.Id; // Set foreign key nếu cần
                }
                await _classForumResultFileRepository.BulkMergeAsync(classForumResultFileNews);
            }
            methodResult.Result = classForumDetailResult;
            return methodResult;
        }

        private async Task<MethodResult<(ClassForumResult, ClassForumDetailResult?)>> UpdateClassForumResultToAttpAsync(CreateClassForumResultCommand request, ClassForum classForum, Course course, ClassForumResult classForumResult, CancellationToken cancellationToken)
        {
            MethodResult<(ClassForumResult, ClassForumDetailResult?)> methodResult = new MethodResult<(ClassForumResult, ClassForumDetailResult?)>();
            var method = await UpdateClassForumDetailResultAsync(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var classForumDetailResult = method.Result;
            if (classForumDetailResult == null)
            {
                methodResult.Result = (classForumResult, classForumDetailResult);
                return methodResult;
            }

            if (request.IsSubmit)
            {
                if (classForumDetailResult.Status != EnumClassForumResultStatus.Draft && classForumResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
                {
                    await _setTimeClassForumDonePublisher.Publish(new Core.Base.BaseModels.BaseQueueModel { QueueId = classForumResult.Id.ToString() }, cancellationToken);
                    classForumResult.TokenFirstTime = await GetTokenAsync(classForum, classForumResult, course.CourseType);
                    if (!classForumResult.IsValid())
                    {
                        methodResult.AddErrorBadRequest(classForumResult.ErrorMessages);
                        return methodResult;
                    }
                    await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
                    });
                    await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            methodResult.Result = (classForumResult, classForumDetailResult);
            return methodResult;
        }

        private async Task<MethodResult<(ClassForumResult, ClassForumDetailResult?)>> CreateClassForumDetailResultAsync(CreateClassForumResultCommand request, ClassForum classForum, ClassForumResult classForumResult, EnumSubmissionCount submissionCount, CancellationToken cancellationToken)
        {
            MethodResult<(ClassForumResult, ClassForumDetailResult?)> methodResult = new MethodResult<(ClassForumResult, ClassForumDetailResult?)>();
            var classForumDetailResult = new ClassForumDetailResult
            {
                Content = request.Content,
                WordContent = request.WordContent,
                Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                ClassForumResultFiles = request.FilePaths?.Select(x => new ClassForumResultFile
                {
                    FilePath = x
                }).ToList() ?? new List<ClassForumResultFile>(),
                SubmissionCount = submissionCount,
                ProcessDate = request.IsSubmit && submissionCount == EnumSubmissionCount.FirstSubmit ? DateTime.UtcNow : null,
                ClassForumResultId = classForumResult.Id,
            };

            classForumDetailResult.MediaType = MediaHelper.GetMediaType(classForumDetailResult.ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault());

            try
            {
                await _classForumDetailResultRepository.BulkMergeAsync(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.ClassForumResultId, c.SubmissionCount, c.IsDeleted };
                });
                var classForumResultFiles = classForumDetailResult.ClassForumResultFiles;
                if (classForumResultFiles.Any())
                {
                    foreach (var file in classForumResultFiles)
                    {
                        file.ClassForumDetailResultId = classForumDetailResult.Id; // Set foreign key nếu cần
                    }
                    await _classForumResultFileRepository.BulkMergeAsync(classForumResultFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate ClassForumDetailResult : {ex.Message}");
            }
            methodResult.Result = (classForumResult, classForumDetailResult);
            return methodResult;
        }

        private async Task<ClassForumResult> CreateClassForumResultAsync(CreateClassForumResultCommand request, ClassForum classForum, Course course, Guid studentId, CancellationToken cancellationToken)
        {
            var classForumResult = new ClassForumResult
            {
                StudentId = studentId,
                LessonResultId = request.LessonResultId,
                ClassForumId = classForum.Id,
                SubmissionCount = EnumSubmissionCount.FirstSubmit,
            };

            if (request.IsSubmit)
            {
                classForumResult.TokenFirstTime = await GetTokenAsync(classForum, classForumResult, course.CourseType);
                try
                {
                    await _classForumResultRepository.BulkMergeAsync(new List<ClassForumResult> { classForumResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId, c.IsDeleted };
                    });
                    await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate ClassForumResult : {ex.Message}");
                }
                await _setTimeClassForumDonePublisher.Publish(new Core.Base.BaseModels.BaseQueueModel { QueueId = classForumResult.Id.ToString() }, cancellationToken);
            }
            else
            {
                await _classForumResultRepository.BulkMergeAsync(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId, c.IsDeleted };
                });
            }
            return classForumResult;
        }

        #endregion Save ClassForum Result

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

        private async Task PublishAIClassForumResponseAsync(ClassForumDetailResult classForumDetailResult, ClassForum classForum, CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            if (classForum.IsAlFeedBack)
            {
                await _submitClassForumGradingPublisher.Publish(new ClassForumAIResponseModel
                {
                    ClassForumResultId = classForumDetailResult.ClassForumResultId,
                    ClassForumDetailResultId = classForumDetailResult.Id,
                    WordContent = request.WordContent,
                    UserAIConfig = classForum.UserAlConfig,
                    SettingModel = classForum.SettingModel,
                    SettingFrequecy = classForum.SettingFrequecy,
                    SettingPresence = classForum.SettingPresence,
                    SettingTemperature = classForum.SettingTemperature,
                    SettingTopP = classForum.SettingTopP,
                    SettingWordMaxLength = classForum.SettingWordMaxLength,
                    SystemRoleAlConfig = classForum.SystemRoleAlConfig,
                    SubmissionCount = classForumDetailResult.SubmissionCount ?? default
                }, cancellationToken);
            }
        }
    }
}
