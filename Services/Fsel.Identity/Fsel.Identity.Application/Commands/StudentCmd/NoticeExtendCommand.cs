// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class NoticeExtendCommand : IRequest<MethodResult<bool>>
    {
    }

    public class NoticeExtendCommandHandler : IRequestHandler<NoticeExtendCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisherl;
        private const int One_Day_Left = 1;
        private const int Two_Days_Left = 2;
        private const int One_Week_Left = 7;
        private const int Two_Weeks_Left = 14;

        public NoticeExtendCommandHandler(IStudentRepository studentRepository, NotificationMessagePublisher notificationMessagePublisherl)
        {
            _studentRepository = studentRepository;
            _notificationMessagePublisherl = notificationMessagePublisherl;
        }

        public async Task<MethodResult<bool>> Handle(NoticeExtendCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Tạo danh sách các tác vụ cho thông báo trước ngày hết hạn
            var dueNotifications = new List<(int days, EnumNotificationContent content)>
            {
                (Two_Days_Left, EnumNotificationContent.NoticeExpireAfterTwoDay),
                (One_Week_Left, EnumNotificationContent.SubcriptionNotice),
                (Two_Weeks_Left, EnumNotificationContent.NoticeExpireAfterTwoWeek)
            };

            // Tạo danh sách các tác vụ cho thông báo sau khi hết hạn
            var expiredNotifications = new List<(int days, EnumNotificationContent content)>
            {
                (One_Day_Left, EnumNotificationContent.UpgradeOrder)
            };

            // Khởi tạo và chạy tất cả các tác vụ đồng thời
            var dueTasks = dueNotifications.Select(n =>
                NoticeStudentsPaymentDue(n.days, n.content, cancellationToken)).ToList();

            var expiredTasks = expiredNotifications.Select(n =>
                NoticeStudentsPaymentAfterExpired(n.days, n.content, cancellationToken)).ToList();

            // Chờ tất cả các tác vụ hoàn thành
            await Task.WhenAll(dueTasks).ConfigureAwait(false);
            await Task.WhenAll(expiredTasks).ConfigureAwait(false);

            // Tạo kết quả và trả về
            return new MethodResult<bool> { StatusCode = StatusCodes.Status200OK };
        }

        private async Task NoticeStudentsPaymentDue(int dayAbsent, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            DateTime targetDate = now.AddDays(-dayAbsent).Date;
            DateTime dateCondition = now.AddDays(-dayAbsent + 1).Date;

            var studentsAbsentIds = await _studentRepository.Queryable
                .GroupBy(x => x.CreatedUserId)
                .Where(g => g.All(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < dateCondition && x.CreatedUserId != Guid.Empty)
                                    && g.Any(x => x.ExpiredDate.HasValue
                                    && x.ExpiredDate.Value.Hour == currentHour
                                    && x.ExpiredDate.Value.Date == targetDate))
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);

        }

        private async Task NoticeStudentsPaymentAfterExpired(int dayExpired, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            DateTime targetDate = now.AddDays(dayExpired).Date;
            DateTime dateCondition = now.AddDays(dayExpired - 1).Date;

            var studentsAbsentIds = await _studentRepository.Queryable
                .GroupBy(x => x.CreatedUserId)
                .Where(g => g.All(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value > dateCondition && x.CreatedUserId != Guid.Empty)
                                    && g.Any(x => x.ExpiredDate.HasValue
                                    && x.ExpiredDate.Value.Hour == currentHour
                                    && x.ExpiredDate.Value.Date == targetDate))
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);

        }

        private async Task SendNotificationMessage(List<Guid>? userIds, EnumNotificationContent content, CancellationToken cancellationToken)
        {

            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = userIds,
                SenderId = Guid.Empty,
                Type = EnumNotificationType.LinkPage,
                Content = content
            };

            await _notificationMessagePublisherl.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
