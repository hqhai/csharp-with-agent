// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd.V1i2
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Models.CommandModels.Ais;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.AIConfigService;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Fsel.Course.Domain.Enums;
    using Fsel.Common.Enums;
    using Polly;

    public class SubmitClassForumAICommand : ClassForumAIResponseModelV2, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassForumAICommand, bool>
    {
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly ISenderService _senderService;
        private const int Max_Time_Retry = 4;
        private const int _intervalRetryTime = 30;
        private readonly IUserService _userService;
        private readonly ILogger<SubmitAIResponseCommandHandler> _logger;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IAIConfigSubmitService _aiConfigSubmitService;

        public SubmitAIResponseCommandHandler(ILessonResultRepository lessonResultRepository,
            SubmitAIResponsePublisher submitAIResponsePublisher,
            NotificationMessagePublisher notificationMessagePublisher,
            IMediator mediator,
            IClassForumDetailResultRepository classForumDetailResultRepository,
            AppSetting appSetting,
            ISenderService senderService,
            IUserService userService,
            ILogger<SubmitAIResponseCommandHandler> logger,
            IClassForumRepository classForumRepository,
            IAIConfigSubmitService aiConfigSubmitService
        )
        {
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _appSetting = appSetting;
            _senderService = senderService;
            _userService = userService;
            _logger = logger;
            _classForumRepository = classForumRepository;
            _aiConfigSubmitService = aiConfigSubmitService;
        }

        public async Task<bool> Handle(SubmitClassForumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var checkForbidden = await _mediator.Send(new CheckForbiddenClassForumCommand { ClassForumDetailResultId = request.ClassForumDetailResultId }, cancellationToken);
            if (checkForbidden != null && checkForbidden.Result)
            {
                return false;
            }

            try
            {
                var options = ConvertJson();

                var classForumDetailResult = await _classForumDetailResultRepository.GetByIdAsync(request.ClassForumDetailResultId);
                if (classForumDetailResult == null)
                {
                    return false;
                }

                var classForumDetailResultOwner = _classForumDetailResultRepository.Queryable
                    .Include(x => x.ClassForumResult)
                    .ThenInclude(x => x.LessonResult)
                    .ThenInclude(x => x.Lesson)
                    .FirstOrDefault(x => x.Id == request.ClassForumDetailResultId);

                #region Retry

                var retryAI = Policy.HandleResult<UserAiModel>(result => result.ClassForumAIs == null || result.ClassForumAIs.Count == 0 || !result.ConditionRetry)
                    .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromMinutes(_intervalRetryTime), async (result, timeSpan, retryCount, context) =>
                    {
                        classForumDetailResult.RetryTime += 1;
                    });


                var retryResult = await retryAI.ExecuteAsync(async () =>
                {
                    var emailUserNeedSupportResult = classForumDetailResultOwner?.CreatedUserId != null
                        ? await _userService.GetStudentByUserIdWithCacheAsync(classForumDetailResultOwner.CreatedUserId)
                        : null;

                    var emailStudent = emailUserNeedSupportResult != null ? emailUserNeedSupportResult!.Content?.Result?.User?.Email : string.Empty;

                    if (classForumDetailResult.RetryTime == Max_Time_Retry)
                    {
                        var model = new SendEmailCommandModel
                        {
                            ToEmails = new List<string> { _appSetting!.CustomerSupportConfig!.Email! },
                            Content =
                                ValueSettings.CustomerSupport.Content.Format(emailStudent, classForumDetailResult.ClassForumResult?.LessonResult?.Lesson?.Name ?? default),
                            Subject = ValueSettings.CustomerSupport.TitleMail.Format(emailStudent ?? default),
                            CcEmails = _appSetting.CustomerSupportConfig.CCEmail
                        };

                        await _senderService.SendEmailAsync(model);
                        return new UserAiModel { ClassForumAIs = null, ConditionRetry = false };
                    }

                    if (classForumDetailResult.ClassForumResult == null)
                    {
                        return new UserAiModel { ClassForumAIs = null, ConditionRetry = false };
                    }

                    var classForum = await _classForumRepository.GetByIdAsync(classForumDetailResult.ClassForumResult.ClassForumId);

                    if (classForum == null)
                    {
                        return new UserAiModel { ClassForumAIs = null, ConditionRetry = false };
                    }

                    // Xác định SubFeatureType dựa trên Layout
                    var subFeatureType = classForum.Layout switch
                    {
                        EnumClassForumLayout.Speaking => EnumSubFeatureType.ClassForumSpeaking,
                        EnumClassForumLayout.Writing => EnumSubFeatureType.ClassForumWriting,
                        _ => throw new InvalidOperationException($"Unknown ClassForumLayout: {classForum.Layout}")
                    };


                    // Sử dụng AIConfigSubmitService để gửi request lên AI


                    var aIResponse = await _aiConfigSubmitService.SubmitByObjectIdAsync(
                        objectId: classForum.Id,
                        content: request.WordContent!,
                        subFeatureType: subFeatureType,
                        featureMultiple: EnumFeatureMultiple.Lesson,
                        cancellationToken: cancellationToken);

                    if (string.IsNullOrWhiteSpace(aIResponse))
                    {
                        return new UserAiModel { ClassForumAIs = null, ConditionRetry = false };
                    }

                    var feedbackElement = GetFeedbackElement(aIResponse);
                    var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAiResponseModel>?>(feedbackElement);

                    bool conditionRetry = classForumAIs?.All(x => x != null) ?? default;

                    return new UserAiModel { ClassForumAIs = classForumAIs, ConditionRetry = conditionRetry };
                });

                #endregion Retry

                classForumDetailResult.GradingAlFeedback = retryResult.ClassForumAIs != null ? ConvertHelper.Serialize(retryResult.ClassForumAIs) : default;

                await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new
                    {
                        c.WordContent,
                        c.Content,
                        c.WordCount,
                        c.SubmissionCount,
                        c.ProcessDate,
                        c.CompletionDate,
                        c.Status,
                        c.ClassForumResultId
                    };
                });

                GetFeatureModuleQuery query = new GetFeatureModuleQuery { FeatureModule = EnumFeatureModule.ClassForumDetailResult, ObjectId = classForumDetailResult.Id, };

                var featureModule = await _mediator.Send(query, cancellationToken);
                var featureModuleResult = featureModule?.Result;

                await _submitAIResponsePublisher.Publish(
                    new SubmitAIResponseModel
                    {
                        GradingAlFeedback = ConvertHelper.Serialize(retryResult.ClassForumAIs),
                        ClassForumResultId = request.ClassForumResultId,
                        EnumSubmissionCount = request.SubmissionCount
                    }, cancellationToken);

                if (retryResult.ClassForumAIs != null && retryResult.ClassForumAIs.Count > 0)
                {
                    if (classForumDetailResultOwner != null)
                    {
                        var paramsLink = new List<object>
                        {
                            featureModuleResult?.CourseId.ToString() ?? string.Empty,
                            featureModuleResult?.UnitId.ToString() ?? string.Empty,
                            featureModuleResult?.LessonId.ToString() ?? string.Empty
                        };

                        NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
                        {
                            ObjectId = classForumDetailResultOwner.Id,
                            UserIds = new List<Guid>() { classForumDetailResultOwner.CreatedUserId },
                            Type = EnumNotificationType.LinkPage,
                            Content = EnumNotificationContent.AIFeedBack,
                            PlatformCode = EnumPlatformCode.LMS,
                            ParamsLink = paramsLink
                        };
                        await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error ClassForumSubmit: {ex}");
            }

            return true;
        }

        private static JsonElement GetFeedbackElement(string? aiResponse)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                throw new ArgumentException("AI response is null or empty", nameof(aiResponse));
            }
            var cleaned = Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aiResponse);

            var model = JsonSerializer.Deserialize<AiJsonResponseModel>(
                cleaned,
                JsonSerializerOptions.Default
            );

            if (model?.Feedback.ValueKind == JsonValueKind.Undefined || model == null)
            {
                throw new InvalidOperationException("Feedback property not found in AI response.");
            }

            return model.Feedback.Clone();
        }

        private static JsonSerializerOptions ConvertJson()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
