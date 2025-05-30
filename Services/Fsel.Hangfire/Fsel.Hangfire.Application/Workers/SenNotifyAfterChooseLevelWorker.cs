using Fsel.Core.Base.Interfaces;
using Fsel.Hangfire.Application.Classes;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Workers
{
    public class SenNotifyAfterChooseLevelWorker : IWorker<NotifyAfterChooseLevel>
    {
        private readonly SendNotifyAfterChooseLevelPublisher _sendNotifyAfterChooseLevelPublisher;

        public SenNotifyAfterChooseLevelWorker(SendNotifyAfterChooseLevelPublisher sendNotifyAfterChooseLevelPublisher)
        {
            _sendNotifyAfterChooseLevelPublisher = sendNotifyAfterChooseLevelPublisher;
        }

        public async Task RunAsync(NotifyAfterChooseLevel? data = null)
        {
            if (data != null)
            {
                await _sendNotifyAfterChooseLevelPublisher.Publish(new SendNotifyAfterChooseLevelQueueModel() { NotifyAfterChooseLevelType = data.AfterChooseLevelType }, CancellationToken.None);
            }
        }
    }
}
