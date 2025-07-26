// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;

    public class SendOtpResultHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<SendOtpResultHandler>
    {
        private readonly ICacheService<SendOtpCountInfo> _sendOtpCountCache;

        public SendOtpResultHandler(ICacheService<SendOtpCountInfo> sendOtpCountCache)
        {
            _sendOtpCountCache = sendOtpCountCache;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            if (context.Step == OtpStep.SendOtp && context.Status)
            {
                ArgumentNullException.ThrowIfNull(context.CountSendOtpCacheKey, nameof(context.CountSendOtpCacheKey));
                ArgumentNullException.ThrowIfNull(context.BlockSendOtpDuration, nameof(context.BlockSendOtpDuration));

                var durationTimeToLimit = context.BlockSendOtpDuration;
                var sendCountInfo = await _sendOtpCountCache.GetAsync(context.CountSendOtpCacheKey);
                if (sendCountInfo == null)
                {
                    sendCountInfo = new SendOtpCountInfo { Count = 1, StartTime = DateTime.UtcNow };
                    sendCountInfo.LastSendTime = sendCountInfo.StartTime;
                }
                else
                {
                    var elapsedTime = DateTime.UtcNow - sendCountInfo.StartTime;
                    if (elapsedTime > context.BlockSendOtpDuration)
                    {
                        // reset the count if the duration has passed
                        sendCountInfo.Count = 1;
                        sendCountInfo.StartTime = DateTime.UtcNow;
                        sendCountInfo.LastSendTime = sendCountInfo.StartTime;
                    }
                    else
                    {
                        sendCountInfo.Count++;
                        durationTimeToLimit -= elapsedTime;
                        sendCountInfo.LastSendTime = DateTime.UtcNow;
                    }
                }

                await _sendOtpCountCache.SetAsync(context.CountSendOtpCacheKey, sendCountInfo, durationTimeToLimit.Value);
            }
        }
    }
}
