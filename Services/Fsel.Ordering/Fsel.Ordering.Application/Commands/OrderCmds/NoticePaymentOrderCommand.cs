// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class NoticePaymentOrderCommand : IRequest<MethodResult<bool>>
    {
    }

    public class NoticePaymentOrderCommandHandler : IRequestHandler<NoticePaymentOrderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private const int RemainTwoDays = 2;
        private const int RemainTwoWeeks = 14;

        public NoticePaymentOrderCommandHandler(IOrderRepository orderRepository, NotificationMessagePublisher notificationMessagePublisher)
        {
            _orderRepository = orderRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(NoticePaymentOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            // Gửi cho học sinh còn 2 ngày sử dụng
            var studentRemainTwoDays = GetOrderQuery(_orderRepository.Queryable, RemainTwoDays).ToList();
            await CreateListUserForSendNotification(studentRemainTwoDays, RemainTwoDays, cancellationToken);

            // Gủi cho học sinh còn 2 tuần sử dụng
            var studentRemainTwoWeeks = GetOrderQuery(_orderRepository.Queryable, RemainTwoWeeks).ToList();
            await CreateListUserForSendNotification(studentRemainTwoWeeks, RemainTwoWeeks, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }

        /// <summary>
        ///  Hàm Query theo điều kiện chung
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="remainDays"></param>
        /// <returns></returns>
        private static IQueryable<T> GetOrderQuery<T>(IQueryable<T> queryable, int remainDays) where T : Order
        {
            DateTime compareWithCreateDate = DateTime.UtcNow.AddDays(remainDays);

            return queryable.Where(x => x.ExpireDate != null && ((DateTime)x.ExpireDate).Date < compareWithCreateDate.Date && ((DateTime)x.ExpireDate).Month < compareWithCreateDate.Month && ((DateTime)x.ExpireDate).Year < compareWithCreateDate.Year);
        }


        /// <summary>
        /// List học sinh sắp hết hạn
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="remainDays"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CreateListUserForSendNotification(List<Order> orders, int remainDays, CancellationToken cancellationToken)
        {
            var groupedOrders = orders
                                 .GroupBy(x => x.IsTrial)
                                 .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).ToList());

            var listUserOnlyTrial = groupedOrders.GetValueOrDefault(true, new List<Guid>())
                .Except(groupedOrders.GetValueOrDefault(false, new List<Guid>()))
                .ToList();

            if (listUserOnlyTrial.Count > 0 && remainDays == RemainTwoDays)
            {
                await SendNotification(listUserOnlyTrial, true, remainDays, cancellationToken);
            }

            await SendNotification(groupedOrders.GetValueOrDefault(false, new List<Guid>()), false, remainDays, cancellationToken);
        }


        /// <summary>
        /// Tạo thông báo
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="isTrial"></param>
        /// <param name="remainDays"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
