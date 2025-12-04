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
            if (context.Status)
            {
                ArgumentNullException.ThrowIfNull(context.CountSendOtpCacheKey, nameof(context.CountSendOtpCacheKey));
                ArgumentNullException.ThrowIfNull(context.SendOtpCountLifeTimeDuration, nameof(context.SendOtpCountLifeTimeDuration));

                var sendCountInfo = await _sendOtpCountCache.GetAsync(context.CountSendOtpCacheKey);
                if (sendCountInfo == null)
                {
                    await CreateSendCountInfo(context.CountSendOtpCacheKey, context.SendOtpCountLifeTimeDuration.Value, context.OtpProviderType.ToString());
                }
                else
                {
                    await UpdateSendCountInfo(sendCountInfo, context.CountSendOtpCacheKey, context.SendOtpCountLifeTimeDuration.Value, context.OtpProviderType.ToString());
                }
            }
        }

        private async Task CreateSendCountInfo(string key, TimeSpan lifeTime, string provider)
        {
            var sendCountInfo = new SendOtpCountInfo
            {
                Count = 1,
                StartTime = DateTime.UtcNow,
                LastProvider = provider,
            };
            sendCountInfo.LastSendTime = sendCountInfo.StartTime;
            await _sendOtpCountCache.SetAsync(key, sendCountInfo, lifeTime);
        }

        private async Task UpdateSendCountInfo(SendOtpCountInfo sendCountInfo, string key, TimeSpan lifeTime, string provider)
        {
            sendCountInfo.LastProvider = provider;
            var elapsedTime = DateTime.UtcNow - sendCountInfo.StartTime;
            if (elapsedTime > lifeTime)
            {
                sendCountInfo.Count = 1;
                sendCountInfo.StartTime = DateTime.UtcNow;
                sendCountInfo.LastSendTime = sendCountInfo.StartTime;
            }
            else
            {
                sendCountInfo.Count++;
                lifeTime -= elapsedTime;
                sendCountInfo.LastSendTime = DateTime.UtcNow;
            }

            await _sendOtpCountCache.SetAsync(key, sendCountInfo, lifeTime);
        }
    }
}
