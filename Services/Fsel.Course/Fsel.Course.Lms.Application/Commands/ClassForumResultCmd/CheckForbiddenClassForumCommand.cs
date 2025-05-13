// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.CommandModels;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.StorageServices.Models;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Polly;

    public class CheckForbiddenClassForumCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassForumDetailResultId { get; set; }
    }

    public class CheckForbiddenClassForumCommandHandler : IRequestHandler<CheckForbiddenClassForumCommand, MethodResult<bool>>
    {
        private readonly ISystemService _systemService;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;
        private readonly IInteractionService _interactionService;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IStorageService _storageService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private const int Max_Time_Retry = 3;
        private const int IntervalRetryTime = 30;

        public CheckForbiddenClassForumCommandHandler(ISystemService systemService,
                                                      AppSetting appSetting,
                                                      IMediator mediator,
                                                      IInteractionService interactionService,
                                                      IClassForumDetailResultRepository classForumDetailResultRepository,
                                                      IStorageService storageService,
                                                      NotificationMessagePublisher notificationMessagePublisher)
        {
            _systemService = systemService;
            _appSetting = appSetting;
            _mediator = mediator;
            _interactionService = interactionService;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _storageService = storageService;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(CheckForbiddenClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumDetailResult = await _classForumDetailResultRepository.Queryable
                                                                                .Include(x => x.ClassForumResultFiles)
                                                                                .Include(x => x.ClassForumResult)
                                                                                .ThenInclude(x => x.ClassForum)
                                                                                .ThenInclude(x => x.Lesson)
                                                                                .FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);
            if (classForumDetailResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumDetailResult));
                return methodResult;
            }

            // check kho từ cấm Fsel
            var listForbiddenWordResultWordContent = await _systemService.CheckContainForbiddenWord(classForumDetailResult.WordContent ?? string.Empty);
            var containsForbiddenWord = listForbiddenWordResultWordContent.Content?.Result ?? Enumerable.Empty<string>().ToList();
            if (containsForbiddenWord.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords), string.Join(", ", containsForbiddenWord));
                return methodResult;
            }

            // checkImage
            var filePaths = classForumDetailResult.ClassForumResultFiles.Select(x => x.FilePath ?? string.Empty).ToList();
            if (filePaths != null && filePaths.Any() && classForumDetailResult.ClassForumResult?.ClassForum?.CourseSkill == EnumCourseSkill.Writing)
            {
                await CheckImages(filePaths, classForumDetailResult);
            }

            // check word
            if (!string.IsNullOrEmpty(classForumDetailResult.WordContent))
            {
                var checkWordContent = await CheckWordContent(classForumDetailResult.WordContent, classForumDetailResult, cancellationToken);
                if (checkWordContent.Result)
                {
                    // thông báo lỗi language
                    NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                    {
                        ObjectId = classForumDetailResult.Id,
                        UserIds = new List<Guid>() { classForumDetailResult.ClassForumResult?.CreatedUserId ?? Guid.Empty },
                        Content = EnumNotificationContent.LanguageNotEnglish,
                        Type = EnumNotificationType.LinkPage,
                        PlatformCode = EnumPlatformCode.LMS
                    };
                    await _notificationMessagePublisher.Publish(model, cancellationToken);
                    methodResult.Result = true;
                }
            }

            // thông báo nếu có từ cấm ảnh hoặc text
            if (classForumDetailResult.IsForbiddenImage || classForumDetailResult.IsForbiddenWork)
            {
                NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                {
                    ObjectId = classForumDetailResult.Id,
                    UserIds = new List<Guid>() { classForumDetailResult.ClassForumResult?.CreatedUserId ?? Guid.Empty },
                    Content = EnumNotificationContent.ForbiddenClassForum,
                    Type = EnumNotificationType.LinkPage,
                    PlatformCode = EnumPlatformCode.LMS,
                    ParamsMessage = new List<object> { classForumDetailResult.ClassForumResult?.ClassForum?.Lesson?.Name ?? string.Empty }
                };
                await _notificationMessagePublisher.Publish(model, cancellationToken);
            }

            await _classForumDetailResultRepository.ExecuteTransactionAsync(async () =>
            {
                _classForumDetailResultRepository.Update(classForumDetailResult);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<bool>> CheckWordContent(string wordContent, ClassForumDetailResult classForumDetailResult, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var retryResult = await CheckForbiddenByChatGpt(wordContent, classForumDetailResult, cancellationToken);
            if (retryResult == null)
            {
                // gửi thông báo null
                NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                {
                    ObjectId = classForumDetailResult.Id,
                    UserIds = new List<Guid>() { classForumDetailResult.ClassForumResult?.CreatedUserId ?? Guid.Empty },
                    Content = EnumNotificationContent.NullForbiddenClassForum,
                    Type = EnumNotificationType.LinkPage,
                    PlatformCode = EnumPlatformCode.LMS
                };
                await _notificationMessagePublisher.Publish(model, cancellationToken);

                return methodResult;
            }

            if (retryResult.Result?.Determination == "LANGUAGE")
            {
                classForumDetailResult.IsForbiddenWork = true;

                if (classForumDetailResult.ClassForumResult?.ClassForum?.CourseSkill == EnumCourseSkill.Writing)
                {
                    methodResult.Result = true;
                    return methodResult;
                }
                else if (classForumDetailResult.ClassForumResult?.ClassForum?.CourseSkill == EnumCourseSkill.Speaking)
                {
                    // call lại stt nếu vẫn trả ra
                    var filePath = classForumDetailResult.ClassForumResultFiles.FirstOrDefault();
                    var convert = await _storageService.ConvertSpeechToTextSetLanguage(new ConvertSpeechToTextSetLanguageModel { FilePath = filePath?.FilePath });
                    if (convert.IsSuccessStatusCode && convert.Content?.Result != null)
                    {
                        classForumDetailResult.WordContent = convert.Content.Result;

                        // call lại check từ cấm
                        retryResult = await CheckForbiddenByChatGpt(convert.Content.Result, classForumDetailResult, cancellationToken);
                        if (retryResult.Result?.Determination == "LANGUAGE")
                        {
                            methodResult.Result = true;
                            return methodResult;
                        }
                    }
                }
            }
            else if (retryResult.Result?.Determination == "INAPPROPRIATE")
            {
                classForumDetailResult.IsForbiddenWork = true;
                return methodResult;
            }

            classForumDetailResult.IsForbiddenWork = false;
            return methodResult;
        }

        private async Task<MethodResult<AIApprovalModel>> CheckForbiddenByChatGpt(string wordContent, ClassForumDetailResult classForumDetailResult, CancellationToken cancellationToken)
        {
            MethodResult<AIApprovalModel> methodResult = new MethodResult<AIApprovalModel>();

            string role = File.ReadAllText(ResourceSettings.AICheckForbiddenWordRole);
            string systemConfig = File.ReadAllText(ResourceSettings.AICheckForbiddenWordInstruction);
            var userAiConfig = role!.Replace("{0}", wordContent ?? string.Empty, StringComparison.CurrentCulture);
            var aiApprovalModel = _appSetting.OpenAiConfig?.CheckForbiddenAIModel;
            int countRetry = 0;

            var retryAI = Policy.HandleResult<AIApprovalModel?>(result => result == null)
                                .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromSeconds(IntervalRetryTime), async (result, timeSpan, retryCount, context) =>
                                {
                                    countRetry += 1;
                                });

            var retryResult = await retryAI.ExecuteAsync(async () =>
            {
                var aIResponse = await _mediator.Send(new AiCmd.SubmitAICommand
                {
                    SettingModel = aiApprovalModel,
                    SettingTemperature = 1,
                    SettingFrequecy = 0,
                    SettingWordMaxLength = 1000,
                    SettingPresence = 0,
                    SettingTopP = 1,
                    SystemRoleAlConfig = userAiConfig,
                    UserAIConfig = systemConfig,
                }, cancellationToken).ConfigureAwait(false);

                if (aIResponse == null)
                {
                    return null;
                }

                var aiApprovalAndComment = ConvertHelper.Deserialize<AIApprovalModel>(Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse));
                if (aiApprovalAndComment == null)
                {
                    return null;
                }

                classForumDetailResult.GradingAiForbidden = aIResponse;
                return aiApprovalAndComment;
            });

            methodResult.Result = retryResult;
            return methodResult;
        }

        private async Task<VoidMethodResult> CheckImages(IList<string> filePaths, ClassForumDetailResult classForumDetailResult)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            foreach (var filePath in filePaths)
            {
                var checkImage = await _interactionService.CheckHarmfulContentImage(new CheckHarmfulContentImageModel { FilePath = filePath });
                if (!checkImage.IsSuccessStatusCode || checkImage.Content?.Result == true)
                {
                    classForumDetailResult.IsForbiddenImage = true;
                    return methodResult;
                }
            }

            return methodResult;
        }
    }
}
