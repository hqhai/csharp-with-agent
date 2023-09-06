using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Ordering.Application.Queues.Publishers
{
    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotificationMessagePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationTypeTextModel notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.OrderingQueue.NameQueue.NotificationTypeText, new NotificationTypeTextModel
            {
                Title = notification.Title,
                ObjectId = notification.ObjectId,
                UserId = notification.UserId,
                Roles = notification.Roles,
                Message = notification.Message,
                UserIds = notification.UserIds,
            }, cancellationToken);
        }
    }
}
