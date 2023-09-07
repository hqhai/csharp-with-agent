using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class InterationActionPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public InterationActionPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(InterationActionQueueModel model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.ClassForum, new InterationActionQueueModel
            {
                ObjectId = model.ObjectId,
                Type = model.Type,
                UserId = model.UserId,
            }, cancellationToken);

        }
    }
}
