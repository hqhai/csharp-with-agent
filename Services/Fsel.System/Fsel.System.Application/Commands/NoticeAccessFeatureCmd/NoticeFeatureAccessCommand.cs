// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.NoticeAccessFeatureCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class NoticeFeatureAccessCommand : IRequest<MethodResult<bool>>
    {
    }

    public class NoticeFeatureAccessCommandHandler : IRequestHandler<NoticeFeatureAccessCommand, MethodResult<bool>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public NoticeFeatureAccessCommandHandler(IFeatureAccessTimeRepository featureAccessTimeRepository, NotificationMessagePublisher notificationMessagePublisher)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(NoticeFeatureAccessCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            await CheckStudentsNoActionForSomeDays(1, EnumNotificationContent.FirstStudyNotice, cancellationToken); //1 ngày không đăng nhập
            await CheckStudentsNoActionForSomeDays(2, EnumNotificationContent.SecondStudyNotice, cancellationToken); //2 ngày không đăng nhập
            await CheckStudentsNoActionForSomeDays(3, EnumNotificationContent.ThirdStudyNotice, cancellationToken); //3 ngày không đăng nhập
            await CheckStudentsNoActionForSomeDays(4, EnumNotificationContent.FourthStudyNotice, cancellationToken);  //4 ngày không đăng nhập
            await CheckStudentsNoActionForSomeDays(5, EnumNotificationContent.FifthStudyNotice, cancellationToken); //5 ngày không đăng nhập
            await CheckStudentsNoActionForSomeDays(15, EnumNotificationContent.StopBothering, cancellationToken); //15 ngày không đăng nhập

            //await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            //{
            //    UserIds = listUserNoAction.Select(x => x.CreatedUserId).ToList(),
            //    Content = EnumNotificationContent.DiscussionBoardInActive,

            //    Type = EnumNotificationType.LinkPage,
            //    PlatformCode = EnumPlatformCode.LMS,
            //    ObjectId = Guid.NewGuid(),
            //}, cancellationToken);

            return methodResult;
        }


        private async Task CheckStudentsNoActionForSomeDays(int dayAbsent, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            DateTime targetDate = now.AddDays(-dayAbsent).Date;
            DateTime dateCondition = now.AddDays(-dayAbsent + 1).Date;

            var studentsAbsentIds = await _featureAccessTimeRepository.Queryable
                .GroupBy(x => x.CreatedUserId) // Nhóm các bản ghi theo CreatedUserId
                .Where(g => g.All(x => x.LastVisited.HasValue && x.LastVisited.Value < dateCondition && x.CreatedUserId != Guid.Empty)
                                    && g.Any(x => x.LastVisited.HasValue // Kiểm tra nếu LastVisited không phải là null
                                    && x.LastVisited.Value.Hour == currentHour
                                    && x.LastVisited.Value.Date == targetDate)) // So sánh giờ, phút và ngày cách đây 48 giờ
                .Select(g => g.Key) // Lấy UserId của những nhóm thỏa mãn điều kiện
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

            await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
