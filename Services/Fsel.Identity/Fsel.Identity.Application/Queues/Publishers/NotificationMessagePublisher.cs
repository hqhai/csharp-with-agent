using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Identity.Application.Queues.Publishers
{
    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotificationMessagePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationSendingQueueModel model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.SendNotification, new NotificationSendingQueueModel
            {
                UserIds = model.UserIds,
                Type = model.Type,
                Content = model.Content,
                PlatformCode = EnumPlatformCode.LMS,
                ParamsLink = model.ParamsLink,
                ParamsMessage = model.ParamsMessage
            }, cancellationToken);
        }
    }
}
