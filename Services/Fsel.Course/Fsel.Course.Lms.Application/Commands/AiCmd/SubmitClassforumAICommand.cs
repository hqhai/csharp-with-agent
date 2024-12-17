// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
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

    public class SubmitClassForumAICommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassForumAICommand, bool>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly SetTimeRetryClassForumPublisher _setTimeRetryClassForumPublisher;
        private readonly ISenderService _senderService;
        private const int Max_Time_Retry = 3;
        private readonly IUserService _userService;
        private readonly ILogger<SubmitAIResponseCommandHandler> _logger;

        public SubmitAIResponseCommandHandler(ILessonResultRepository lessonResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher, NotificationMessagePublisher notificationMessagePublisher, IMediator mediator, IClassForumDetailResultRepository classForumDetailResultRepository, SetTimeRetryClassForumPublisher setTimeRetryClassForumPublisher, IMapper mapper, AppSetting appSetting, ISenderService senderService, IUserService userService, ILogger<SubmitAIResponseCommandHandler> logger)
        {
            _lessonResultRepository = lessonResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _setTimeRetryClassForumPublisher = setTimeRetryClassForumPublisher;
            _mapper = mapper;
            _appSetting = appSetting;
            _senderService = senderService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<bool> Handle(SubmitClassForumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var options = new JsonSerializerOptions
            {
                Converters = { (JsonConverter)new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} Start");

            var classForumDetailResult = await _classForumDetailResultRepository.GetByIdAsync(request.ClassForumDetailResultId);
            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 1: {classForumDetailResult.Serialize(options)}");

            var userAiConfig = request!.UserAIConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = request.SettingModel,
                SettingTemperature = request.SettingTemperature,
                SettingFrequecy = request.SettingFrequecy,
                SettingWordMaxLength = request.SettingWordMaxLength,
                SettingPresence = request.SettingPresence,
                SettingTopP = request.SettingTopP,
                SystemRoleAlConfig = request.SystemRoleAlConfig,
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);

            #region Retry


            aIResponse = Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);

            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} userAiConfig: {userAiConfig}");
            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} aIResponse: {aIResponse}");

            var classForumAIs = GetClassForumAIs(ConvertHelper.Deserialize<List<ClassForumAIModel>?>(aIResponse));

            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumAIs: {classForumAIs.Serialize()}");

            bool conditionRetry = classForumAIs?.All(x => x != null) ?? default;

            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} conditionRetry: {conditionRetry}");

            var classForumDetailResultOwner = _classForumDetailResultRepository.Queryable.Include(x => x.ClassForumResult).ThenInclude(x => x.LessonResult).ThenInclude(x => x.Lesson).FirstOrDefault(x => x.Id == request.ClassForumDetailResultId);

            var emailUserNeedSupportResult = classForumDetailResultOwner?.CreatedUserId != null ? await _userService.GetStudentByUserIdAsync(classForumDetailResultOwner.CreatedUserId) : null;

            var emailStudent = emailUserNeedSupportResult != null ? emailUserNeedSupportResult!.Content?.Result?.Human?.Email : string.Empty;

            if (classForumDetailResult != null && classForumDetailResult.RetryTime > Max_Time_Retry)
            {
                SendEmailCommandModel model = new SendEmailCommandModel
                {
                    ToEmails = new List<string> { _appSetting!.CustomerSupportConfig!.Email! },
                    Content = ValueSettings.CustomerSupport.Content.Format(emailStudent, classForumDetailResult.ClassForumResult?.LessonResult?.Lesson?.Name ?? default),
                    Subject = ValueSettings.CustomerSupport.TitleMail.Format(emailStudent ?? default),
                    CcEmails = _appSetting.CustomerSupportConfig.CCEmail
                };
                await _senderService.SendEmailAsync(model);
            }

            if ((classForumAIs == null || classForumAIs.Count == 0 || !conditionRetry) && classForumDetailResult != null && classForumDetailResult.RetryTime <= Max_Time_Retry)
            {
                var model = _mapper.Map<SetTimeRetryClassForumModel>(request);
                model.StartDate = DateTime.UtcNow;
                classForumDetailResult.RetryTime += 1;
                await _setTimeRetryClassForumPublisher.Publish(model, cancellationToken);
            }
            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 2: {classForumDetailResult.Serialize(options)}");

            #endregion Retry

            if (classForumDetailResult != null)
            {
                classForumDetailResult.GradingAlFeedback = classForumAIs != null ? ConvertHelper.Serialize(classForumAIs) : default;
                _classForumDetailResultRepository.Update(classForumDetailResult, false
                , x => x.WordContent, x => x.Content
                , x => x.WordCount, x => x.SubmissionCount
                , x => x.ProcessDate, x => x.CompletionDate, x => x.Status);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} classForumDetailResult 3: {classForumDetailResult.Serialize(options)}");
            _logger.LogInformation($"SubmitAIResponseCommand Id: {request.ClassForumDetailResultId} End");

            GetFeatureModuleQuery query = new GetFeatureModuleQuery
            {
                FeatureModule = EnumFeatureModule.ClassForumDetailResult,
                ObjectId = classForumDetailResult?.Id ?? default,
            };

            var featureModule = await _mediator.Send(query, cancellationToken);
            var featureModuleResult = featureModule?.Result;

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = ConvertHelper.Serialize(classForumAIs),
                ClassForumResultId = request.ClassForumResultId,
                EnumSubmissionCount = request.SubmissionCount
            }, cancellationToken);

            if (!string.IsNullOrEmpty(aIResponse))
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
