namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SendNotifyBuyBlindBoxPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SendNotifyBuyBlindBoxPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SendNotifyBuyBlindBoxModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.SendNotifyBuyBlindBox, request, cancellationToken);
        }
    }
}
