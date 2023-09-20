using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotificationMessagePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationQueueModel notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                return;
            }


            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.SendNotification, new NotificationQueueModel
            {
                ObjectId = notification.ObjectId,
                Message = notification.Message,
                Link = notification.Link,
                UserId = notification.UserId,
                Type = notification.Type,
                Content = notification.Content,
                ParamsMessage = notification.ParamsMessage,
                SenderId = notification.SenderId,
            }, cancellationToken);

        }
    }
}
