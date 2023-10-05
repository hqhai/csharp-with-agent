using Fsel.Core.Base.Interfaces;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class DiscussionBoardLikePublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly IInteractionActionRepository _interactionActionRepository;

        public DiscussionBoardLikePublisher(IQueueProvider queueProvider, IInteractionActionRepository interactionActionRepository)
        {
            _queueProvider = queueProvider;
            _interactionActionRepository = interactionActionRepository;
        }

        public async Task Publish(Guid? objectId, CancellationToken cancellationToken)
        {
            if (objectId == null)
            {
                return;
            }
            var likeNumber = await _interactionActionRepository.Queryable.Where(x => x.ObjectId == objectId && x.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, new DiscussionBoardQueueModel
            {
                ObjectId = objectId.Value,
                LikeNumber = likeNumber,
            }, cancellationToken);

        }
    }
}
