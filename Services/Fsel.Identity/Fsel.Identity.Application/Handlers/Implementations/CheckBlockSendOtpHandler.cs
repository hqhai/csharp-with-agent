// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;

    public class CheckBlockSendOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<CheckBlockSendOtpHandler>
    {
        private readonly ICacheService<SendOtpCountInfo> _sendOtpCountCache;

        public CheckBlockSendOtpHandler(ICacheService<SendOtpCountInfo> sendOtpCountCache)
        {
            _sendOtpCountCache = sendOtpCountCache;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            ArgumentNullException.ThrowIfNull(context.GapSendDuration, nameof(context.GapSendDuration));
            ArgumentNullException.ThrowIfNull(context.SendOtpCountLifeTimeDuration, nameof(context.SendOtpCountLifeTimeDuration));
            ArgumentNullException.ThrowIfNull(context.MaxCountOtpSend, nameof(context.MaxCountOtpSend));

            var sendCountInfo = await _sendOtpCountCache.GetAsync(context.CountSendOtpCacheKey);
            if (sendCountInfo != null)
            {
                if (IsExpiredSendCounter(sendCountInfo.StartTime, context.SendOtpCountLifeTimeDuration.Value))
                {
                    await _sendOtpCountCache.RemoveAsync(context.CountSendOtpCacheKey);
                }
                else if (HasReachedMaxSendCount(sendCountInfo.Count, context.MaxCountOtpSend.Value)
                    || GetGapSendDuration(sendCountInfo.LastSendTime, context.GapSendDuration.Value).HasValue)
                {
                    context.Status = false;
                    return;
                }
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }

        public static TimeSpan? GetGapSendDuration(DateTime lastSentTime, TimeSpan gapDuration)
        {
            var durationFromLastSent = DateTime.UtcNow.Subtract(lastSentTime);
            var remainDuration = gapDuration - durationFromLastSent;
            if (remainDuration > TimeSpan.Zero)
            {
                return remainDuration;
            }

            return null;
        }

        public static bool HasReachedMaxSendCount(int currentCount, int maxCount)
        {
            return currentCount >= maxCount;
        }

        public static bool IsExpiredSendCounter(DateTime createdDateCounter, TimeSpan durationLifeTime)
        {
            var durationFromCreated = DateTime.UtcNow.Subtract(createdDateCounter);
            var remainToExpiredDuration = durationLifeTime - durationFromCreated;

            return remainToExpiredDuration < TimeSpan.Zero;
        }
    }
}
