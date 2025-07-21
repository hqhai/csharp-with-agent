using Fsel.Core.Base;
using Fsel.Hangfire.Application.Classes;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Http;

namespace Fsel.Hangfire.Application.Workers
{
    public class PushNoticeWorker : BaseWorker<PushNoticeTime>
    {
        private readonly PushNoticePublisher _sendNotifyAfterChooseLevelPublisher;

        public PushNoticeWorker(PushNoticePublisher sendNotifyAfterChooseLevelPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _sendNotifyAfterChooseLevelPublisher = sendNotifyAfterChooseLevelPublisher;
        }

        public override async Task RunAsync(PushNoticeTime? data)
        {
            if (data != null)
            {
                await _sendNotifyAfterChooseLevelPublisher.Publish(new PushNoticeQueueModel() { TimeNotifyType = data.TimeNotifyType }, CancellationToken.None);
            }
        }
    }
}
