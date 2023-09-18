using Fsel.Core.Base.Interfaces;
using Fsel.Notification.Domain.Model.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Notification.Application.Queues.Publishers
{
    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotificationMessagePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationMessageModel notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.NotificationQueue.NameQueue.Notification, new NotificationQueueModel
            {
                ObjectId = notification.ObjectId,
                UserId = notification.UserId,
                Message = notification.Message,
                Link = notification.Link,
                UserIds = notification.UserIds,
                AvatarPath = notification.AvatarPath,
                SenderId = notification.SenderId,
                Status = notification.Status,
            }, cancellationToken);
        }
    }
}
