using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using Microsoft.Extensions.Logging;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<NotificationMessagePublisher> _logger;

        public NotificationMessagePublisher(IQueueProvider queueProvider, ILogger<NotificationMessagePublisher> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
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
                ParamsLink = notification.ParamsLink,
                PlatformCode = notification.PlatformCode,
            }, cancellationToken);

            _logger.LogInformation($"NotificationMessagePublisher: {notification.UserId}");
        }
    }
}
