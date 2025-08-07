namespace Fsel.Ordering.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class AddCoinWhenCoursePurchasedPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public AddCoinWhenCoursePurchasedPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(AddCoinWhenCoursePurchasedCommandModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.OrderingQueue.NameQueue.AddCoinWhenCoursePurchased, request, cancellationToken);
        }
    }
}
