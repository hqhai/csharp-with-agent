// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

            return queryable.Where(x => x.ExpireDate != null && ((DateTime)x.ExpireDate).Date == compareWithCreateDate.Date && ((DateTime)x.ExpireDate).Month == compareWithCreateDate.Month && ((DateTime)x.ExpireDate).Year == compareWithCreateDate.Year);
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
            var studentTrialIds = orders.Where(x => x.IsTrial).Select(x => x.UserId).ToList(); // học sinh đã và đang học thử
            var listUserOnlyTrial = GetListStudentIsTrialing(_orderRepository.Queryable, studentTrialIds); // học sinh học thử, chưa thanh toán

            if (listUserOnlyTrial.Count > 0)
            {
                await SendNotification(listUserOnlyTrial, cancellationToken);
            }
        }

        /// <summary>
        /// Lấy danh sách học sinh đang học thử
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        private static List<Guid> GetListStudentIsTrialing<T>(IQueryable<T> queryable, List<Guid> studentIds) where T : Order
        {
            var exceptStudent = queryable.Where(x => !x.IsTrial && studentIds.Contains(x.UserId)).Select(x => x.UserId).ToList();

            List<Guid> newStudentIds = studentIds.Except(exceptStudent).ToList();

            return newStudentIds;
        }

        /// <summary>
        /// Tạo thông báo
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="isTrial"></param>
        /// <param name="remainDays"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendNotification(IList<Guid> userIds, CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                UserIds = userIds,
                Type = EnumNotificationType.LinkPage,
                Content = EnumNotificationContent.NoticePayment,
                PlatformCode = EnumPlatformCode.LMS
            };
            await _notificationMessagePublisher.Publish(model, cancellationToken);
        }
    }
}
