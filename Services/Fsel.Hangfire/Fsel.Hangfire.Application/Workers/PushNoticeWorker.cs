using Fsel.Core.Base;
using Fsel.Hangfire.Application.Classes;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fsel.Hangfire.Application.Workers
{
    public class PushNoticeWorker : BaseWorker<PushNoticeTime>
    {
        private readonly PushNoticePublisher _sendNotifyAfterChooseLevelPublisher;
        private readonly ILogger<PushNoticeWorker> _logger;

        public PushNoticeWorker(PushNoticePublisher sendNotifyAfterChooseLevelPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor, ILogger<PushNoticeWorker> logger) : base(authContext, httpContextAccessor)
        {
            _sendNotifyAfterChooseLevelPublisher = sendNotifyAfterChooseLevelPublisher;
            _logger = logger;
        }

        public override async Task RunAsync(PushNoticeTime? data)
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
