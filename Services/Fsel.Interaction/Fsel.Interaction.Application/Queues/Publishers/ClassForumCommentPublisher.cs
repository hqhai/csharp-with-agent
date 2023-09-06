using Fsel.Core.Base.Interfaces;
using Fsel.Interaction.Domain.IRepositories;
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

        public async Task Publish(Guid? objectId, CancellationToken cancellationToken)
        {
            if (objectId == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.Comment, new ClassForumCommentQueueModel
            {
                ObjectId = objectId.Value
            }, cancellationToken);

        }
    }
}
