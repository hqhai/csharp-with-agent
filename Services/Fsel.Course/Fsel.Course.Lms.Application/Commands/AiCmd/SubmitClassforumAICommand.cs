// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using ClassForumResultCmd;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
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

    public class SubmitClassForumAiCommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAiResponseCommandHandler : IRequestHandler<SubmitClassForumAiCommand, bool>
    {
        private readonly SubmitAIResponsePublisher _submitAiResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ISenderService _senderService;
        private const int MaxTimeRetry = 4;
        private const int IntervalRetryTime = 30;
        private const string NameSchema = "criteria_schema";
        private readonly IUserService _userService;
        private readonly ILogger<SubmitAiResponseCommandHandler> _logger;

        public SubmitAiResponseCommandHandler(
            SubmitAIResponsePublisher submitAiResponsePublisher,
            NotificationMessagePublisher notificationMessagePublisher,
            IMediator mediator,
            IClassForumDetailResultRepository classForumDetailResultRepository,
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            AppSetting appSetting,
            ISenderService senderService,
            IUserService userService,
            IClassForumRepository classForumRepository,
            ILogger<SubmitAiResponseCommandHandler> logger)
        {
            _submitAiResponsePublisher = submitAiResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _appSetting = appSetting;
            _senderService = senderService;
            _userService = userService;
            _logger = logger;
            _classForumRepository = classForumRepository;
        }

        private class UserAiModel
        {
            public IList<ClassForumAIModel>? ClassForumAIs { get; set; }
            public bool ConditionRetry { get; set; }
        }

        public async Task<bool> Handle(SubmitClassForumAiCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var checkForbidden = await _mediator.Send(new CheckForbiddenClassForumCommand { ClassForumDetailResultId = request.ClassForumDetailResultId }, cancellationToken);
                if (checkForbidden.Result)
                {
                    _logger.LogWarning("SubmitAIResponseCommand - Content is forbidden for ClassForumDetailResultId: {ClassForumDetailResultId}", request.ClassForumDetailResultId);
                    return false;
                }

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
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

                var retryAi = Policy.HandleResult<UserAiModel>(result => result.ClassForumAIs == null || result.ClassForumAIs.Count == 0 || !result.ConditionRetry)
                    .WaitAndRetryAsync(MaxTimeRetry, _ => TimeSpan.FromMinutes(IntervalRetryTime), async (_, _, _, _) =>
                    {
                        classForumDetailResult.RetryTime += 1;
                    });

                var retryResult = await retryAi.ExecuteAsync(async () =>
                {
                    var emailUserNeedSupportResult = classForumDetailResultOwner?.CreatedUserId != null
                        ? await _userService.GetStudentByUserIdWithCacheAsync(classForumDetailResultOwner.CreatedUserId)
                        : null;

                    var emailStudent = emailUserNeedSupportResult != null ? emailUserNeedSupportResult!.Content?.Result?.User?.Email : string.Empty;

                    if (classForumDetailResult.RetryTime == MaxTimeRetry)
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

                    var classForum = await _classForumRepository.GetByIdAsync(classForumDetailResult.ClassForumResult.ClassForumId);
                    if (classForum?.AiPromptCriteriaId == null)
                    {
                        _logger.LogWarning("SubmitAIResponseCommand Id: {Id} ClassForum or AiPromptCriteriaId is null", request.ClassForumDetailResultId);
                        return new UserAiModel { ClassForumAIs = null, ConditionRetry = false };
                    }

                    var aiConfig = await _aiCriteriaConfigRepository.ReadQueryable
                        .Include(x => x.AiPromptManager)
                        .FirstOrDefaultAsync(x => x.Id == classForum.AiPromptCriteriaId, cancellationToken);

                    var aIResponse = await _mediator
                        .Send(
                            new V1i1.SubmitAICommand
                            {
                                SettingModel = request.SettingModel,
                                SettingTemperature = request.SettingTemperature,
                                SettingFrequecy = request.SettingFrequecy,
                                SettingWordMaxLength = request.SettingWordMaxLength,
                                SettingPresence = request.SettingPresence,
                                SettingTopP = request.SettingTopP,
                                SystemRoleAlConfig = request.SystemRoleAlConfig,
                                UserAIConfig = aiConfig.SettingAiConfig,
                                Text = aiConfig.SettingAiJson,
                                NameSchema = NameSchema
                            }, cancellationToken).ConfigureAwait(false);

                    aIResponse = Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
                    var doc = JsonDocument.Parse(aIResponse);
                    var items = doc.RootElement.GetProperty("parameters");

                    _logger.LogCritical($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} userAiConfig: {aiConfig.SettingAiJson}");

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} userAiConfig: {aiConfig.SettingAiJson}");
                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} aIResponse: {aIResponse}");

                    var classForumAIs = GetClassForumAIs(ConvertHelper.Deserialize<List<ClassForumAIModel>?>(items));

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumAIs: {classForumAIs.Serialize()}");

                    bool conditionRetry = classForumAIs?.All(x => x != null) ?? default;

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} conditionRetry: {conditionRetry}");

                    _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 1: {classForumDetailResult.Serialize(options)}");

                    return new UserAiModel { ClassForumAIs = classForumAIs, ConditionRetry = conditionRetry };
                });

                #endregion Retry

                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 2: {classForumDetailResult.Serialize(options)}");

                classForumDetailResult.GradingAlFeedback = retryResult.ClassForumAIs != null ? ConvertHelper.Serialize(retryResult.ClassForumAIs) : default;
                _classForumDetailResultRepository.Update(classForumDetailResult, false
                    , x => x.WordContent, x => x.Content
                    , x => x.WordCount, x => x.SubmissionCount
                    , x => x.ProcessDate, x => x.CompletionDate
                    , x => x.Status
                    // task 5307 chưa lên prod
                    //, x => x.IsForbiddenImage
                    //, x => x.IsForbiddenWork, x => x.GradingAiForbidden
                    , x => x.PronunciationAlFeedback);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 3: {classForumDetailResult.Serialize(options)}");
                _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} End");

                var query = new GetFeatureModuleQuery { FeatureModule = EnumFeatureModule.ClassForumDetailResult, ObjectId = classForumDetailResult.Id, };

                var featureModule = await _mediator.Send(query, cancellationToken);
                var featureModuleResult = featureModule?.Result;

                await _submitAiResponsePublisher.Publish(
                    new SubmitAIResponseModel
                    {
                        GradingAlFeedback = retryResult.ClassForumAIs?.Serialize(),
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

                        var notificationQueue = new NotificationSendingQueueModel()
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
