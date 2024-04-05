// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly SubmitClassForumGradingPublisher _submitClassForumGradingPublisher;
        private readonly ISystemService _systemService;
        private readonly IClassforumDetailResultRepository _classforumDetailResultRepository;

        public CreateClassForumResultCommandHandler(IMapper mapper, CreateTokenHistoryPublisher createTokenHistoryPublisher, ICourseRepository courseRepository, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, SubmitClassForumGradingPublisher submitClassForumGradingPublisher, ISystemService systemService, IClassforumDetailResultRepository classforumDetailResultRepository)
        {
            _mapper = mapper;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _courseRepository = courseRepository;
            _authContext = authContext;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _submitClassForumGradingPublisher = submitClassForumGradingPublisher;
            _systemService = systemService;
            _classforumDetailResultRepository = classforumDetailResultRepository;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumResultModel> methodResult = new MethodResult<ClassForumResultModel>();

            // Check từ khoá cấm
            var listForbiddenWordResultContent = await _systemService.CheckContainForbiddenWord(request.Content);
            var listForbiddenWordResultWordContent = await _systemService.CheckContainForbiddenWord(request.WordContent);
            var containsForbiddenWord = (listForbiddenWordResultContent.Content?.Result ?? Enumerable.Empty<string>())
             .Concat(listForbiddenWordResultWordContent.Content?.Result ?? Enumerable.Empty<string>()).Distinct()
             .ToList();

            if (containsForbiddenWord.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords), string.Join(", ", containsForbiddenWord));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var studentId = student.Id;
            var classForumResult = await _classForumResultRepository.Queryable
                    .Include(x => x.ClassForumDetailResults)
                    .Include(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumScores)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);
            if (classForumResult != null)
            {
                var classForumDetailResults = await _classforumDetailResultRepository.Queryable.Where(x => x.ClassForumResultId == classForumResult.Id).ToListAsync(cancellationToken);
                if (classForumDetailResults.Count > 2)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumDetailHaveMoreThan2));
                    return methodResult;
                }
            }

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                List<ClassForumResultFile> classForumResultFiles = new List<ClassForumResultFile>();
                if (classForumResult == null)
                {
                    var classForumDetailResults = new List<ClassForumDetailResult>
                    {
                        new ClassForumDetailResult
                        {
                            Content = request.Content,
                            WordContent = request.WordContent,
                            Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                            ClassForumResultFiles = classForumResultFiles
                        }
                    };

                    if (request.FilePaths != null)
                    {
                        classForumResultFiles = request.FilePaths!.Select(x => new ClassForumResultFile
                        {
                            FilePath = x,
                            ClassForumDetailResultId = classForumResult!.Id
                        }).ToList();
                    }

                    classForumResult = new ClassForumResult
                    {
                        StudentId = studentId,
                        LessonResultId = request.LessonResultId,
                        ClassForumId = classForum.Id,
                        ClassForumDetailResults = classForumDetailResults,
                    };

                    classForumDetailResults.FirstOrDefault()!.MediaType = MediaHelper.GetMediaType(classForumResultFiles.Select(x => x.FilePath).FirstOrDefault());

                    classForumResult = await GetClassForumResultToSubmissionCount(classForumResult, classForum, course.CourseType, cancellationToken);
                    classForumResult = _classForumResultRepository.Add(classForumResult);
                    if (classForumDetailResults.FirstOrDefault()?.Status == EnumClassForumResultStatus.Draft)
                    {
                        await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Draft || classForumResult.Status == EnumClassForumResultStatus.Denied)
                {
                    _mapper.Map(request, classForumResult);
                    classForumResult.ClassForumDetailResults.FirstOrDefault()!.Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft;
                    classForumResult.GradingAlFeedback = null;

                    if (request.FilePaths != null)
                    {
                        classForumResult.ClassForumDetailResults.FirstOrDefault()!.ClassForumResultFiles = request.FilePaths.Select(x => new ClassForumResultFile
                        {
                            FilePath = x,
                        }).ToList();
                    }
                    classForumResult = await GetClassForumResultToSubmissionCount(classForumResult, classForum, course.CourseType, cancellationToken);
                    classForumResult = _classForumResultRepository.Update(classForumResult);
                    await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumHasSubmitted));
                    return methodResult;
                }
                //check AI feedback
                if (classForum.IsAlFeedBack)
                {
                    await _submitClassForumGradingPublisher.Publish(new ClassForumAIResponseModel
                    {
                        ClassForumResultId = classForumResult.Id,
                        ClassForumDetailResultId = classForumResult.Id,
                        WordContent = request.WordContent,
                        UserAIConfig = classForum.UserAlConfig,
                        SettingModel = classForum.SettingModel,
                        SettingFrequecy = classForum.SettingFrequecy,
                        SettingPresence = classForum.SettingPresence,
                        SettingTemperature = classForum.SettingTemperature,
                        SettingTopP = classForum.SettingTopP,
                        SettingWordMaxLength = classForum.SettingWordMaxLength,
                        SystemRoleAlConfig = classForum.SystemRoleAlConfig,
                    }, cancellationToken);
                }

                //mặc định gửi cho tất cả CSO
                IList<EnumRole> roles = new List<EnumRole>();
                roles.Add(EnumRole.CSO);

                NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                {
                    ObjectId = classForumResult.Id,
                    Roles = roles,
                    Content = EnumNotificationContent.CreateClassForumResult,
                    Type = EnumNotificationType.Text,
                    SenderId = _authContext.CurrentUserId,
                    PlatformCode = EnumPlatformCode.LMSAdmin
                };

                await _notificationMessagePublisher.Publish(model, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });
            if (classForumResult != null && classForumResult.TokenFirstTime.HasValue && classForumResult.TokenFirstTime.Value > 0 &&
                classForumResult.Status == EnumClassForumResultStatus.Pending && classForumResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
            {
                var tokenHistorys = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = classForumResult.Id,
                        VolatileToken = classForumResult.TokenFirstTime.Value,
                        Feature = EnumTokenFeature.Learn,
                        Mission = GetTokenMission(classForum,classForumResult),
                        Type = EnumTokenHistoryType.Recevived,
                        UserId = student.Human?.UserId ?? default,
                    }
                };
                await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken);
            }
            return methodResult;
        }

        private static EnumTokenMission GetTokenMission(ClassForum classForum, ClassForumResult classForumResult)
        {
            return classForum.CourseSkill == EnumCourseSkill.Writing ? EnumTokenMission.ClassForumWriting
               : classForumResult.MediaType == EnumMediaType.Video ? EnumTokenMission.ClassForumSpeakingVideo
               : EnumTokenMission.ClassForumSpeakingAudio;
        }

        private async Task<ClassForumResult> GetClassForumResultToSubmissionCount(ClassForumResult classForumResult, ClassForum classForum, EnumCourseType courseType, CancellationToken cancellationToken)
        {
            if (classForumResult.Status == EnumClassForumResultStatus.Pending && !classForumResult.SubmissionCount.HasValue)
            {
                classForumResult.SubmissionCount = EnumSubmissionCount.FirstSubmit;
                var token = await GetToken(GetTokenMission(classForum, classForumResult), courseType);
                classForumResult.TokenFirstTime = token;
            }
            else if (classForumResult.Status == EnumClassForumResultStatus.Pending && classForumResult.SubmissionCount.HasValue)
            {
                classForumResult.SubmissionCount = EnumSubmissionCount.SecondSubmit;
            }
            return classForumResult;
        }

        private async Task<int> GetToken(EnumTokenMission tokenMission, EnumCourseType courseType)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.Learn,
                Mission = tokenMission,
                CourseType = courseType
            });
            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigs.Content?.Result;
            return (int)(tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default);
        }
    }
}
