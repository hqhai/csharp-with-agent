using Fsel.Core.Base.Interfaces;
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

        public async Task Publish(Guid? objectId,Guid userId, string message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.Notification, new NotificationQueueModel
            {
                ObjectId = objectId!.Value,
                UserId = userId,
                Message = message,

            }, cancellationToken);
        }
    }
}
