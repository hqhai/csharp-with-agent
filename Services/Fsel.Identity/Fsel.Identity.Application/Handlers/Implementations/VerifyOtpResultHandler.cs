// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Core.Localization;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Shared.Helpers;
    using Microsoft.Extensions.Localization;

    public class VerifyOtpResultHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<VerifyOtpResultHandler>
    {
        private readonly ICacheService<string> _cache;
        private readonly ICacheService<FailedCountInfo> _cacheFailedCount;
        private readonly IStringLocalizer _stringLocalizer;

        public VerifyOtpResultHandler(ICacheService<string> cache,
            ICacheService<FailedCountInfo> cacheFailedCount,
            IStringLocalizer stringLocalizer)
        {
            _cache = cache;
            _cacheFailedCount = cacheFailedCount;
            _stringLocalizer = stringLocalizer;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            ArgumentNullException.ThrowIfNull(context.OtpBlockDuration, nameof(context.OtpBlockDuration));
            if (context.Step == OtpStep.VerifyOtp)
            {
                if (context.Status)
                {
                    await _cache.RemoveAsync(context.OtpCacheKey);
                }
                else
                {
                    var failedCountInfo = await CountFailedVerifyOtp(context.CountFailedVerifyOtpCacheKey, context.OtpBlockDuration.Value);

                    if (failedCountInfo != null && failedCountInfo.Count >= context.MaxCountVerifyFail)
                    {
                        ArgumentNullException.ThrowIfNull(context.OtpBlockDuration);
                        _ = await BlockOtp(context.BlockedOtpCacheKey, context.OtpBlockDuration.Value);
                        await _cacheFailedCount.RemoveAsync(context.CountFailedVerifyOtpCacheKey);
                        context.ErrorMessage = _stringLocalizer["i18n_OTP_reach_max_verify"].Value.InjectParam(context.MaxCountOtpSend.ToString(), context.OtpBlockDuration.Value.Minutes.ToString());
                        return;
                    }
                    else
                    {
                        context.ErrorMessage = _stringLocalizer["i18n_OTP_is_not_valid"];
                    }
                }
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }

        public async Task<FailedCountInfo?> CountFailedVerifyOtp(string cacheKey, TimeSpan duration)
        {
            ArgumentNullException.ThrowIfNull(cacheKey, nameof(cacheKey));

            var failedCountInfo = await _cacheFailedCount.GetAsync(cacheKey);
            if (failedCountInfo == null)
            {
                failedCountInfo = new FailedCountInfo
                {
                    Count = 1,
                    StartTime = DateTime.UtcNow,
                    LastFailedTime = DateTime.UtcNow
                };
            }
            else
            {
                failedCountInfo.Count++;
                failedCountInfo.LastFailedTime = DateTime.UtcNow;
            }

            var elapsedTime = DateTime.UtcNow - failedCountInfo.StartTime;
            if (elapsedTime > duration)
            {
                await _cacheFailedCount.RemoveAsync(cacheKey);
                return null;
            }
            else
            {
                duration -= elapsedTime;
            }
            await _cacheFailedCount.SetAsync(cacheKey, failedCountInfo, duration);
            return failedCountInfo;
        }

        private async Task<DateTime> BlockOtp(string cacheKey, TimeSpan blockTime)
        {
            ArgumentNullException.ThrowIfNull(cacheKey, nameof(cacheKey));
            var blockToTime = DateTime.UtcNow.Add(blockTime);
            await _cache.SetAsync(cacheKey, blockToTime.ToShortTimeString(), blockTime);
            return blockToTime;
        }
    }
}
