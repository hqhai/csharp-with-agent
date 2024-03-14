// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.NoticeAccessFeatureCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.IRepositories;
    using MediatR;

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

            DateTime sevenDaysAgo = DateTime.Today.AddDays(-7);

            var listUserNoAction = _featureAccessTimeRepository.Queryable.Where(x => x.EnumFeature == EnumFeature.DiscussionBoard && x.LastVisited < sevenDaysAgo).ToList();


            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = listUserNoAction.Select(x => x.CreatedUserId).ToList(),
                Content = EnumNotificationContent.DiscussionBoardInActive,
                Type = EnumNotificationType.LinkPage,
                PlatformCode = EnumPlatformCode.LMS,
                ObjectId = Guid.NewGuid(),
            }, cancellationToken);

            return methodResult;
        }
    }
}
