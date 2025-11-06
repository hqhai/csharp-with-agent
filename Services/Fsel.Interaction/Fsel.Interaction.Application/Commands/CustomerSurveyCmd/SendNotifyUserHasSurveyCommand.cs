// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.OrderService;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendNotifyUserHasSurveyCommand : SaveUserSurveyAssignmentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendNotifyUserHasSurveyCommandHandler : IRequestHandler<SendNotifyUserHasSurveyCommand, MethodResult<bool>>
    {
        private readonly IOrderService _orderService;
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public SendNotifyUserHasSurveyCommandHandler(IOrderService orderService, ISurveyConfigRepository surveyConfigRepository, AuthContext authContext, IUserService userService, NotificationMessagePublisher notificationMessagePublisher)
        {
            _orderService = orderService;
            _surveyConfigRepository = surveyConfigRepository;
            _authContext = authContext;
            _userService = userService;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(SendNotifyUserHasSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentStatusResult = await _orderService.GetCurrentStatusAsync(_authContext.CurrentUserId);
            var studentStatus = studentStatusResult.Content?.Result;
            if (studentStatus == null || studentStatus.Value == EnumTrialRegistrationStatus.New || studentStatus.Value == EnumTrialRegistrationStatus.Finished)
            {
                return methodResult;
            }
            var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var @event = eventResults.Content?.Result?.FirstOrDefault();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var surveyConfigEntities = await _surveyConfigRepository.Queryable.Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.Status == EnumSurveyConfigStatus.Active).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            var surveyConfigs = surveyConfigEntities.Where(p => p.ProgressRequirements != null && p.ProgressRequirements.Any(x => x.CourseType == request.CourseType && x.ProgressRequirement == request.ProgressRequirement)).ToList();

            surveyConfigs = surveyConfigs.Where(p => p.ApplicableSubjects != null && p.ApplicableSubjects.Any(x => x.CourseLevel == request.CourseLevel && x.ApplicableSubjects != null && x.ApplicableSubjects.Any(n => n == GetStatus(studentStatus.Value)))).ToList();

            var surveyConfig = @event != null ? surveyConfigs.Where(p =>
                                                                    p.ApplicablePrograms?.Contains(EnumSurveyFormType.Event) == true &&
                                                                    p.CompetitionEventIds?.Contains(@event.Id) == true)
                                                                    .OrderByDescending(p => p.CreatedDate)
                                                                    .FirstOrDefault()
                                                : null;

            surveyConfig ??= surveyConfigs
                .Where(p => p.ApplicablePrograms?.Contains(EnumSurveyFormType.Default) == true)
                .OrderByDescending(p => p.CreatedDate)
                .FirstOrDefault();

            if (surveyConfig != null)
            {
                await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel()
                {
                    UserIds = new List<Guid>() { _authContext.CurrentUserId },
                    Type = EnumNotificationType.LinkPage,
                    Content = EnumNotificationContent.SurveyAssignment,
                    ObjectId = surveyConfig.Id,
                    PlatformCode = EnumPlatformCode.LMS
                }, cancellationToken);
            }

            methodResult.Result = true;
            return methodResult;
        }

        private static EnumSurveyConfigApplicableSubject? GetStatus(EnumTrialRegistrationStatus status)
        {
            return status switch
            {
                EnumTrialRegistrationStatus.Trial => EnumSurveyConfigApplicableSubject.Trial,
                EnumTrialRegistrationStatus.Payment => EnumSurveyConfigApplicableSubject.InProgress,
                EnumTrialRegistrationStatus.Expired => EnumSurveyConfigApplicableSubject.Expired,
                _ => null
            };
        }
    }
}
