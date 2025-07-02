// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class SubmitClassForumAICommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassForumAICommand, bool>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly ISenderService _senderService;
        private const int Max_Time_Retry = 4;
        private const int _intervalRetryTime = 30;
        private const string NameSchema = "criteria_schema";
        private readonly IUserService _userService;
        private readonly ILogger<SubmitAIResponseCommandHandler> _logger;

        public SubmitAIResponseCommandHandler(ILessonResultRepository lessonResultRepository,
                                              SubmitAIResponsePublisher submitAIResponsePublisher,
                                              NotificationMessagePublisher notificationMessagePublisher,
                                              IMediator mediator,
                                              IClassForumDetailResultRepository classForumDetailResultRepository,
                                              AppSetting appSetting,
                                              ISenderService senderService,
                                              IUserService userService,
                                              ILogger<SubmitAIResponseCommandHandler> logger)
        {
            _lessonResultRepository = lessonResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _appSetting = appSetting;
            _senderService = senderService;
            _userService = userService;
            _logger = logger;
        }

        private class UserAiModel
        {
            public IList<ClassForumAIModel>? ClassForumAIs { get; set; }
            public bool ConditionRetry { get; set; }
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
                var options = new JsonSerializerOptions
                {
                    Converters = { (JsonConverter)new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                };

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

                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} Start");

                #region Retry

                var retryAI = Policy.HandleResult<UserAiModel>(result => result.ClassForumAIs == null || result.ClassForumAIs.Count == 0 || !result.ConditionRetry)
                                    .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromMinutes(_intervalRetryTime), async (result, timeSpan, retryCount, context) =>
                                    {
                                        classForumDetailResult.RetryTime += 1;
                                    });

                var retryResult = await retryAI.ExecuteAsync(async () =>
                {
                    var emailUserNeedSupportResult = classForumDetailResultOwner?.CreatedUserId != null ? await _userService.GetStudentByUserIdWithCacheAsync(classForumDetailResultOwner.CreatedUserId) : null;

                    var emailStudent = emailUserNeedSupportResult != null ? emailUserNeedSupportResult!.Content?.Result?.Human?.Email : string.Empty;

                    if (classForumDetailResult != null && classForumDetailResult.RetryTime == Max_Time_Retry)
                    {
                        SendEmailCommandModel model = new SendEmailCommandModel
                        {
                            ToEmails = new List<string> { _appSetting!.CustomerSupportConfig!.Email! },
                            Content = ValueSettings.CustomerSupport.Content.Format(emailStudent, classForumDetailResult.ClassForumResult?.LessonResult?.Lesson?.Name ?? default),
                            Subject = ValueSettings.CustomerSupport.TitleMail.Format(emailStudent ?? default),
                            CcEmails = _appSetting.CustomerSupportConfig.CCEmail
                        };
                        await _senderService.SendEmailAsync(model);

                        return new UserAiModel
                        {
                            ClassForumAIs = null,
                            ConditionRetry = false
                        };
                    }

                    var userAiConfig = request!.UserAIConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SuccessCriteriaSchema);
                    var successCriteriaSchema = ConvertHelper.DeserializeFromFilePath<object>(path);

                    var aIResponse = await _mediator.Send(new V1i1.SubmitAICommand
                    {
                        SettingModel = request.SettingModel,
                        SettingTemperature = request.SettingTemperature,
                        SettingFrequecy = request.SettingFrequecy,
                        SettingWordMaxLength = request.SettingWordMaxLength,
                        SettingPresence = request.SettingPresence,
                        SettingTopP = request.SettingTopP,
                        SystemRoleAlConfig = request.SystemRoleAlConfig,
                        UserAIConfig = userAiConfig,
                        Text = successCriteriaSchema,
                        NameSchema = NameSchema
                    }, cancellationToken).ConfigureAwait(false);

                    aIResponse = Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
                    var doc = JsonDocument.Parse(aIResponse);
                    var items = doc.RootElement.GetProperty("parameters");

                    _logger.LogCritical($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} userAiConfig: {userAiConfig}");

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} userAiConfig: {userAiConfig}");
                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} aIResponse: {aIResponse}");

                    var classForumAIs = GetClassForumAIs(ConvertHelper.Deserialize<List<ClassForumAIModel>?>(items));

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumAIs: {classForumAIs.Serialize()}");

                    bool conditionRetry = classForumAIs?.All(x => x != null) ?? default;

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} conditionRetry: {conditionRetry}");

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 1: {classForumDetailResult.Serialize(options)}");

                    return new UserAiModel
                    {
                        ClassForumAIs = classForumAIs,
                        ConditionRetry = conditionRetry
                    };
                });

                #endregion Retry

                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 2: {classForumDetailResult.Serialize(options)}");

                classForumDetailResult.GradingAlFeedback = retryResult.ClassForumAIs != null ? ConvertHelper.Serialize(retryResult.ClassForumAIs) : default;

                await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.WordContent, c.Content, c.WordCount, c.SubmissionCount, c.ProcessDate, c.CompletionDate, c.Status, c.ClassForumResultId };
                });

                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 3: {classForumDetailResult.Serialize(options)}");
                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} End");

                GetFeatureModuleQuery query = new GetFeatureModuleQuery
                {
                    FeatureModule = EnumFeatureModule.ClassForumDetailResult,
                    ObjectId = classForumDetailResult.Id,
                };

                var featureModule = await _mediator.Send(query, cancellationToken);
                var featureModuleResult = featureModule?.Result;

                await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
                {
                    GradingAlFeedback = ConvertHelper.Serialize(retryResult.ClassForumAIs),
                    ClassForumResultId = request.ClassForumResultId,
                    EnumSubmissionCount = request.SubmissionCount
                }, cancellationToken);

                if (retryResult.ClassForumAIs != null && retryResult.ClassForumAIs.Count > 0)
                {
                    if (classForumDetailResultOwner != null)
                    {
                        var lessonResult = await _lessonResultRepository.GetIncludeByIdAsync(classForumDetailResultOwner.ClassForumResult!.LessonResultId);

                        var paramsLink = new List<object> { featureModuleResult?.CourseId.ToString() ?? string.Empty, featureModuleResult?.UnitId.ToString() ?? string.Empty, featureModuleResult?.LessonId.ToString() ?? string.Empty };

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

        private static IList<ClassForumAIModel>? GetClassForumAIs(List<ClassForumAIModel>? classForumAIs)
        {
            if (classForumAIs == null || !classForumAIs.Any())
            {
                return classForumAIs;
            }

            foreach (var item in classForumAIs)
            {
                item.SuccessCriteriaItemFix = ConvertDataToStrings(item.SuccessCriteriaItemFix);
                item.SuccessCriteriaItemEvidence = ConvertDataToStrings(item.SuccessCriteriaItemEvidence);
            }
            return classForumAIs;
        }

        private static IList<string> ConvertDataToStrings(object? data)
        {
            var listStr = data.Deserialize<IList<string>>();
            if (listStr != null)
            {
                return listStr.ToList();
            }
            return new List<string> { data?.ToString() ?? string.Empty };
        }
    }
}
