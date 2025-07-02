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
                (Two_Weeks_Left, EnumNotificationContent.NoticeExpireAfterTwoWeek),
                (-One_Day_Left, EnumNotificationContent.UpgradeOrder)
            };

            foreach (var dueNotification in dueNotifications)
            {
                await NoticeStudentsPaymentDue(dueNotification.days, dueNotification.content, cancellationToken);
            }

            // Tạo kết quả và trả về
            return new MethodResult<bool> { StatusCode = StatusCodes.Status200OK };
        }

        private async Task NoticeStudentsPaymentDue(int dayAbsent, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            DateTime targetDate = now.AddDays(dayAbsent).Date;

            var studentsAbsentIds = await _studentRepository.Queryable
                .Where(x => x.ExpiredDate.HasValue
                                    && x.ExpiredDate.Value.Hour == currentHour
                                    && x.ExpiredDate.Value.Date == targetDate && x.User != null)
                .Select(g => g != null ? g.UserId : Guid.Empty)
                .ToListAsync(cancellationToken);

            await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);

        }

        //private async Task NoticeStudentsPaymentAfterExpired(int dayExpired, EnumNotificationContent content, CancellationToken cancellationToken)
        //{
        //    DateTime now = DateTime.Now;
        //    int currentHour = now.Hour;
        //    DateTime targetDate = now.AddDays(-dayExpired).Date;

        //    var studentsAbsentIds = await _studentRepository.Queryable
        //        .Where(x => x.ExpiredDate.HasValue && x.CreatedUserId != Guid.Empty
        //                            && x.ExpiredDate.Value.Hour == currentHour
        //                            && x.ExpiredDate.Value.Date == targetDate)
        //        .Select(g => g.Human)
        //        .ToListAsync(cancellationToken);

        //    await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);

        //}

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
