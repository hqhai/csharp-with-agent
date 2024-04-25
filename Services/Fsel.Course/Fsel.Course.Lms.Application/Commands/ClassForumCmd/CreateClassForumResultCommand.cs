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
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly SetTimeClassForumDonePublisher _setTimeClassForumDonePublisher;

        public CreateClassForumResultCommandHandler(IMapper mapper, CreateTokenHistoryPublisher createTokenHistoryPublisher, ICourseRepository courseRepository, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, SubmitClassForumGradingPublisher submitClassForumGradingPublisher, ISystemService systemService, IClassForumDetailResultRepository classForumDetailResultRepository, SetTimeClassForumDonePublisher setTimeClassForumDonePublisher)
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
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _setTimeClassForumDonePublisher = setTimeClassForumDonePublisher;
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
                    .Include(x => x.ClassForumDetailResults.OrderBy(x => x.CreatedDate))
                    .ThenInclude(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumScores)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);

            var classForumDetailResult = _classForumDetailResultRepository.Queryable.Include(x => x.ClassForumResultFiles).AsQueryable();

            var classForumDetailResultAttemp1 = classForumDetailResult.Where(x => x.SubmissionCount == EnumSubmissionCount.FirstSubmit && classForumResult != null && x.Status == EnumClassForumResultStatus.Pending && x.ClassForumResultId == classForumResult.Id).FirstOrDefault();

            if (classForumDetailResultAttemp1 != null && classForumDetailResultAttemp1.ProcessDate >= DateTime.UtcNow.AddHours(2))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            if (classForumResult != null)
            {
                var classForumDetailResultModels = await _classForumDetailResultRepository.Queryable.Where(x => x.ClassForumResultId == classForumResult!.Id).ToListAsync(cancellationToken);

                if (classForumDetailResultModels.Count > 2)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumDetailHaveMoreThan2));
                    return methodResult;
                }
            }

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (classForumResult == null)
                {
                    classForumResult = new ClassForumResult
                    {
                        StudentId = studentId,
                        LessonResultId = request.LessonResultId,
                        ClassForumId = classForum.Id,
                        SubmissionCount = EnumSubmissionCount.FirstSubmit,
                    };

                    var classForumDetailResult = new ClassForumDetailResult
                    {
                        Content = request.Content,
                        WordContent = request.WordContent,
                        Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                        ClassForumResultFiles = request.FilePaths?.Select(x => new ClassForumResultFile
                        {
                            FilePath = x
                        }).ToList() ?? new List<ClassForumResultFile>(),
                        SubmissionCount = EnumSubmissionCount.FirstSubmit,
                        ProcessDate = DateTime.UtcNow,
                    };
                    classForumDetailResult.MediaType = MediaHelper.GetMediaType(classForumDetailResult.ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault());
                    classForumResult.ClassForumDetailResults.Add(classForumDetailResult);

                    if (classForumDetailResult.Status != EnumClassForumResultStatus.Draft)
                    {
                        classForumResult.TokenFirstTime = await GetTokenAsync(classForum, classForumResult, course.CourseType);
                    }

                    classForumResult = _classForumResultRepository.Add(classForumResult);
                    if (classForumResult.Status == EnumClassForumResultStatus.Draft)
                    {
                        await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    //await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    if (classForumDetailResult.Status != EnumClassForumResultStatus.Draft && classForumResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
                    {
                        await _setTimeClassForumDonePublisher.Publish(new Core.Base.BaseModels.BaseQueueModel { QueueId = classForumResult.Id.ToString() }, cancellationToken);
                    }

                    await PublishAIClassForumResponseAsync(classForumDetailResult.Id, classForum, request, cancellationToken);
                }
                else if (classForumDetailResultAttemp1 != null && classForumDetailResultAttemp1.ProcessDate <= classForumDetailResultAttemp1.ProcessDate!.Value.AddHours(2))
                {
                    var classForumDetailResult = new ClassForumDetailResult
                    {
                        Content = request.Content,
                        WordContent = request.WordContent,
                        Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                        ClassForumResultFiles = request.FilePaths?.Select(x => new ClassForumResultFile
                        {
                            FilePath = x
                        }).ToList() ?? new List<ClassForumResultFile>(),
                        SubmissionCount = EnumSubmissionCount.SecondSubmit,
                        ProcessDate = DateTime.UtcNow,
                        ClassForumResultId = classForumResult.Id,
                    };

                    classForumDetailResult.MediaType = MediaHelper.GetMediaType(classForumDetailResult.ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault());

                    _classForumDetailResultRepository.Add(classForumDetailResult);
                    await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    if (request.IsSubmit)
                    {
                        await PublishAIClassForumResponseAsync(classForumDetailResult.Id, classForum, request, cancellationToken);
                    }
                }
                else if (classForumDetailResult.Where(x => x.Id == request.ClassForumDetailResultId && x.ClassForumResultId == classForumResult.Id).FirstOrDefault()?.Status == EnumClassForumResultStatus.Draft)
                {
                    var classForumDetailResult = await _classForumDetailResultRepository.Queryable.Where(x => x.Id == request.ClassForumDetailResultId && x.ClassForumResultId == classForumResult!.Id).FirstOrDefaultAsync(cancellationToken);
                    _mapper.Map(request, classForumDetailResult);
                    classForumDetailResult!.Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft;

                    if (request.FilePaths != null)
                    {
                        classForumDetailResult.ClassForumResultFiles = request.FilePaths.Select(x => new ClassForumResultFile
                        {
                            FilePath = x,
                        }).ToList();
                    }

                    classForumDetailResult = _classForumDetailResultRepository.Update(classForumDetailResult);
                    await _classForumDetailResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    if (request.IsSubmit)
                    {
                        await PublishAIClassForumResponseAsync(classForumDetailResult.Id, classForum, request, cancellationToken);
                    }

                    if (classForumDetailResult.Status != EnumClassForumResultStatus.Draft && classForumResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
                    {
                        await _setTimeClassForumDonePublisher.Publish(new Core.Base.BaseModels.BaseQueueModel { QueueId = classForumResult.Id.ToString() }, cancellationToken);

                        classForumResult.TokenFirstTime = await GetTokenAsync(classForum, classForumResult, course.CourseType);

                        classForumDetailResult = _classForumDetailResultRepository.Update(classForumDetailResult);
                        await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        classForumResult = _classForumResultRepository.Update(classForumResult);
                        await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                /* //mặc định gửi cho tất cả CSO
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

                 await _notificationMessagePublisher.Publish(model, cancellationToken);*/
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            var classForumDetailResultFirstSubmit = classForumDetailResult.Where(x => x.ClassForumResultId == classForumResult!.Id && x.SubmissionCount == EnumSubmissionCount.FirstSubmit).FirstOrDefault();
            if (classForumResult != null && classForumDetailResultFirstSubmit?.Status == EnumClassForumResultStatus.Pending)
            {
                var tokenHistorys = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = classForumResult.Id,
                        VolatileToken = classForumResult.TokenFirstTime!.Value,
                        Feature = EnumTokenFeature.Learn,
                        Mission = GetTokenMission(classForum,classForumDetailResultFirstSubmit),
                        Type = EnumTokenHistoryType.Recevived,
                        UserId = student.Human?.UserId ?? default,
                    }
                };
                await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken);
            }
            return methodResult;
        }

        private static EnumTokenMission GetTokenMission(ClassForum classForum, ClassForumDetailResult classForumDetailResult)
        {
            return classForum.CourseSkill == EnumCourseSkill.Writing ? EnumTokenMission.ClassForumWriting
               : classForumDetailResult.MediaType == EnumMediaType.Video ? EnumTokenMission.ClassForumSpeakingVideo
               : EnumTokenMission.ClassForumSpeakingAudio;
        }

        private async Task<int?> GetTokenAsync(ClassForum classForum, ClassForumResult classForumResult, EnumCourseType courseType)
        {
            var classForumDetailResult = classForumResult.ClassForumDetailResults.Where(x => x.ClassForumResultId == classForumResult.Id).FirstOrDefault();
            if (classForumDetailResult != null && classForumDetailResult.Status == EnumClassForumResultStatus.Pending)
            {
                var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
                {
                    Feature = EnumTokenFeature.Learn,
                    Mission = GetTokenMission(classForum, classForumDetailResult),
                    CourseType = courseType
                });
                if (!tokenConfigs.IsSuccessStatusCode)
                {
                    return default;
                }
                var tokenConfig = tokenConfigs.Content?.Result;
                return (int?)(tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default);
            }
            return default;
        }

        private async Task<Guid> PublishAIClassForumResponseAsync(Guid classForumDetailResultId, ClassForum classForum, CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            if (classForum.IsAlFeedBack)
            {
                await _submitClassForumGradingPublisher.Publish(new ClassForumAIResponseModel
                {
                    ClassForumDetailResultId = classForumDetailResultId,
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
            return classForumDetailResultId;
        }
    }
}
