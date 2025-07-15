using Fsel.Core.Base.Interfaces;
using Fsel.Hangfire.Application.Classes;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Workers
{
    public class PushNoticeWorker : IWorker<PushNoticeTime>
    {
        private readonly PushNoticePublisher _sendNotifyAfterChooseLevelPublisher;

        public PushNoticeWorker(PushNoticePublisher sendNotifyAfterChooseLevelPublisher)
        {
            _sendNotifyAfterChooseLevelPublisher = sendNotifyAfterChooseLevelPublisher;
        }

        public async Task RunAsync(PushNoticeTime? data = null)
        {
            if (data != null)
            {
                await _sendNotifyAfterChooseLevelPublisher.Publish(new PushNoticeQueueModel() { TimeNotifyType = data.TimeNotifyType }, CancellationToken.None);
            }
        }
    }
}
