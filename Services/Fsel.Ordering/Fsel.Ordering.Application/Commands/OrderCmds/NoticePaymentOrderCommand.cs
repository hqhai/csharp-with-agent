// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class NoticePaymentOrderCommand : IRequest<MethodResult<bool>>
    {
        public int RemainDays { get; set; }

        public bool IsTrial { get; set; }
    }

    public class NoticePaymentOrderCommandHandler : IRequestHandler<NoticePaymentOrderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        public NoticePaymentOrderCommandHandler(IOrderRepository orderRepository, NotificationMessagePublisher notificationMessagePublisher)
        {
            _orderRepository = orderRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(NoticePaymentOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            DateTime compareWithCreateDate = DateTime.UtcNow.AddDays(request.RemainDays);

            var studentOrdersResults = _orderRepository.Queryable.Where(x => x.ExpireDate != null && ((DateTime)x.ExpireDate).Date <= compareWithCreateDate.Date && ((DateTime)x.ExpireDate).Month <= compareWithCreateDate.Month && ((DateTime)x.ExpireDate).Year <= compareWithCreateDate.Year);

            var studentTrialIds = studentOrdersResults.Where(x => x.IsTrial).Select(x => x.UserId).ToList();

            var studentOrders = studentOrdersResults.Where(x => !x.IsTrial).Select(x => x.UserId).ToList();


            IList<Guid> userIds = studentOrders.ToList();

            if (request.IsTrial)
            {
                userIds = studentTrialIds.Except(studentOrders).ToList();
            }

            await SendNotification(userIds, request.IsTrial, request.RemainDays, cancellationToken);

            return methodResult;
        }

        private async Task SendNotification(IList<Guid> userIds, bool isTrial, int remainDays, CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                UserIds = userIds,
                Type = EnumNotificationType.LinkPage,
                Content = isTrial ? EnumNotificationContent.NoticePayment : (remainDays == 2 ? EnumNotificationContent.NoticeExpireAfterTwoDay : EnumNotificationContent.NoticeExpireAfterTwoDay),
                PlatformCode = EnumPlatformCode.LMS
            };
            await _notificationMessagePublisher.Publish(model, cancellationToken);
        }
    }
}
