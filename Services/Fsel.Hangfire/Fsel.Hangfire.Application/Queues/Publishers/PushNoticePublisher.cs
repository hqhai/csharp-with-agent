using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class PushNoticePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public PushNoticePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(PushNoticeQueueModel model, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.PushNotice, model, cancellationToken);
        }
    }
}
