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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
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
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly SubmitClassForumGradingPublisher _submitClassForumGradingPublisher;
        private readonly ISystemService _systemService;
        private readonly IMediator _mediator;

        public CreateClassForumResultCommandHandler(IMapper mapper, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, ISystemService systemService, IMediator mediator, SubmitClassForumGradingPublisher submitClassForumGradingPublisher)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _systemService = systemService;
            _mediator = mediator;
            _submitClassForumGradingPublisher = submitClassForumGradingPublisher;
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

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);

            if (!student.IsSuccessStatusCode)
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

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == lessonResult.LessonId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var classForumResult = await _classForumResultRepository.Queryable
                    .Include(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumScores)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (classForumResult == null)
                {
                    classForumResult = new ClassForumResult
                    {
                        Content = request.Content,
                        StudentId = studentId ?? default,
                        LessonResultId = request.LessonResultId,
                        Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                        ClassForumId = classForum.Id,
                        WordContent = request.WordContent,
                    };

                    if (request.FilePaths != null)
                    {
                        classForumResult.ClassForumResultFiles = request.FilePaths.Select(x => new ClassForumResultFile
                        {
                            FilePath = x,
                        }).ToList();
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
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Draft || classForumResult.Status == EnumClassForumResultStatus.Denied)
                {
                    _mapper.Map(request, classForumResult);
                    classForumResult.Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft;
                    classForumResult.GradingAlFeedback = null;

                    if (request.FilePaths != null)
                    {
                        classForumResult.ClassForumResultFiles = request.FilePaths.Select(x => new ClassForumResultFile
                        {
                            FilePath = x,
                        }).ToList();
                    }

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

            return methodResult;
        }
    }
}
