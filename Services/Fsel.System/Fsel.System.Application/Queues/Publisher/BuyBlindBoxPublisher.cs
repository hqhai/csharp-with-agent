namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;

    public class BuyBlindBoxPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public BuyBlindBoxPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(BuyBlindBoxCommandModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.BuyBlindBox, request, cancellationToken);
        }
    }
}
