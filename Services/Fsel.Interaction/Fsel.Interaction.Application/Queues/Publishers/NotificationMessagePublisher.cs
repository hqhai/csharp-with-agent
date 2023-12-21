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

        public async Task Publish(NotificationSendingQueueModel notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                return;
            }


            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.SendNotification, new NotificationSendingQueueModel
            {
                ObjectId = notification.ObjectId,
                Type = notification.Type,
                UserIds = notification.UserIds,
                Roles = notification.Roles,
                Content = notification.Content,
                SenderId = notification.SenderId,
                ParamsMessage = notification.ParamsMessage,
                ParamsLink = notification.ParamsLink,
            }, cancellationToken);

        }
    }
}
