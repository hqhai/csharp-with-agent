// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRegistrationCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateExpireTrialStudentCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateExpireTrialStudentCommandHandler : IRequestHandler<UpdateExpireTrialStudentCommand, MethodResult<bool>>
    {
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private const int CompareDate = -12;

        public UpdateExpireTrialStudentCommandHandler(IStudentTrialRegistrationRepository studentTrialRegistrationRepository, NotificationMessagePublisher notificationMessagePublisher)
        {
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(UpdateExpireTrialStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            DateTime compareWithCreateDate = DateTime.UtcNow.AddDays(CompareDate);
            var studentTrialRegistrationResults = _studentTrialRegistrationRepository.Queryable.Where(x => x.Status == EnumTrialRegistrationStatus.Trial && x.CreatedDate.Date <= compareWithCreateDate.Date && x.CreatedDate.Month <= compareWithCreateDate.Month && x.CreatedDate.Year <= compareWithCreateDate.Year);

            if (studentTrialRegistrationResults == null)
            {
                methodResult.Result = true;
                return methodResult;
            }

            List<Guid> expireUserId = studentTrialRegistrationResults.Select(x => x.UserId).ToList();
            await SendNotification(expireUserId, cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SendNotification(List<Guid> expireTrialUserId,CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                Content = EnumNotificationContent.NoticePayment,
                Type = EnumNotificationType.LinkPage,
                UserIds = expireTrialUserId,
                PlatformCode = EnumPlatformCode.LMS
            };

            await _notificationMessagePublisher.Publish(model, cancellationToken);
        }
    }
}
