using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class SendNotifyAfterChooseLevelPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SendNotifyAfterChooseLevelPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SendNotifyAfterChooseLevelQueueModel model, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SendNotifyAfterChooseLevel, model, cancellationToken);
        }
    }
}
