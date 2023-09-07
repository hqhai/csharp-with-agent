using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class ClassForumCommentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ClassForumCommentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationQueueModel notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                return;
            }


            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.Comment, new NotificationQueueModel
            {
                ObjectId = notification.ObjectId,
                Message = notification.Message,
                Link = notification.Link,
                UserId = notification.UserId,
                Type = notification.Type,
                Content = notification.Content,
                ParamsMessage = notification.ParamsMessage,

            }, cancellationToken);

        }
    }
}
