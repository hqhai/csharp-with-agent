using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Ordering.Application.Queues.Publishers
{
    public class FinishTrialRegistrationPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public FinishTrialRegistrationPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(FinishSubmissionQueueModel finishSubmisstion, CancellationToken cancellationToken)
        {
            if (finishSubmisstion == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.OrderingQueue.NameQueue.FinishSubmission, finishSubmisstion, cancellationToken);
        }
    }

}
