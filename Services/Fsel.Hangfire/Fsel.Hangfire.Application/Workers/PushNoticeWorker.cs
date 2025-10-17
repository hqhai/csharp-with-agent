using Fsel.Core.Base.Interfaces;
using Fsel.Hangfire.Application.Classes;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.Extensions.Logging;

namespace Fsel.Hangfire.Application.Workers
{
    public class PushNoticeWorker : IWorker<PushNoticeTime>
    {
        private readonly PushNoticePublisher _sendNotifyAfterChooseLevelPublisher;
        private readonly ILogger<PushNoticeWorker> _logger;

        public PushNoticeWorker(PushNoticePublisher sendNotifyAfterChooseLevelPublisher, ILogger<PushNoticeWorker> logger)
        {
            _sendNotifyAfterChooseLevelPublisher = sendNotifyAfterChooseLevelPublisher;
            _logger = logger;
        }

        public async Task RunAsync(PushNoticeTime? data = null)
        {
            if (data != null)
            {
                _logger.LogError($"StartQueue_PushNotice_{data.TimeNotifyType}");
                await _sendNotifyAfterChooseLevelPublisher.Publish(new PushNoticeQueueModel() { TimeNotifyType = data.TimeNotifyType }, CancellationToken.None);
            }
            else
            {
                _logger.LogError($"Data_PushNotice_Null");
            }
        }
    }
}
