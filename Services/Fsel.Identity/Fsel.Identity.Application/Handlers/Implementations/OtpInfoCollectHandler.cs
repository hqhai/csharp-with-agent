// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;

    public interface IOtpDataCollector
    {
        Task<OtpSessionInfo> GetOtpSessionInfo(string identity, string purpose);
    }
    public class OtpInfoCollectHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<OtpInfoCollectHandler>, IOtpDataCollector
    {
        private readonly ICacheService<SendOtpCountInfo> _sendOtpCountCache;
        private readonly ICacheService<string> _cache;
        private ICacheService<FailedCountInfo> _cacheFailedCount;

        public OtpInfoCollectHandler(ICacheService<SendOtpCountInfo> sendOtpCountCache,
            ICacheService<string> cache,
            ICacheService<FailedCountInfo> cacheFailedCount)
        {
            _sendOtpCountCache = sendOtpCountCache;
            _cache = cache;
            _cacheFailedCount = cacheFailedCount;
        }

        public async Task<OtpSessionInfo> GetOtpSessionInfo(string identity, string purpose)
        {
            if (string.IsNullOrEmpty(identity) || string.IsNullOrEmpty(purpose))
            {
                return null;
            }

            if (!Enum.TryParse(purpose, ignoreCase: true, out OtpPurpose result))
            {
                return null;
            }

            var context = new OtpPipelineContext(identity, result, OtpStep.CollectData);

            await CollectData(context);

            return context.OtpSessionInfo;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            if (Next != null)
            {
                await Next.Handle(context);
            }

            if (!(context.Status && context.Step == OtpStep.VerifyOtp))
            {
                await CollectData(context);
            }
        }

        private async Task CollectData(OtpPipelineContext context)
        {
            try
            {
                var otpInfo = new OtpSessionInfo();
                context.OtpSessionInfo = otpInfo;


                var otp = await _cache.GetAsync(context.OtpCacheKey);
                if (otp == null)
                {
                    otpInfo.OtpExpired = true;
                }
                var value = await _cache.GetAsync(context.BlockedOtpCacheKey);
                if (value != null)
                {
                    if (!DateTime.TryParse(value, out var blockToTime)
                        || blockToTime > DateTime.UtcNow)
                    {
                        otpInfo.IsOtpBlocked = true;
                        otpInfo.WaitTimeDuration = blockToTime.Subtract(DateTime.UtcNow);
                        return;
                    }
                }
                await CollectSendOtpInfo(context);
                await CollectVerifyOtpInfo(context);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task CollectSendOtpInfo(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context.MaxCountOtpSend, nameof(context.MaxCountOtpSend));
            ArgumentNullException.ThrowIfNull(context.GapSendDuration, nameof(context.GapSendDuration));
            var sendCountInfo = await _sendOtpCountCache.GetAsync(context.CountSendOtpCacheKey);
            if (sendCountInfo != null)
            {
                var sendInfo = new SendInfo();
                context.OtpSessionInfo.SendInfo = sendInfo;
                sendInfo.Provider = sendCountInfo.LastProvider;
                if (CheckBlockSendOtpHandler.HasReachedMaxSendCount(sendCountInfo.Count, context.MaxCountOtpSend.Value))
                {
                    sendInfo.IsBlockedByReachMaxSendCount = true;
                    sendInfo.WaitTimeDuration = context.SendOtpCountLifeTimeDuration - DateTime.UtcNow.Subtract(sendCountInfo.StartTime);
                }
                else
                {
                    var gapDuration = CheckBlockSendOtpHandler.GetGapSendDuration(sendCountInfo.LastSendTime, context.GapSendDuration.Value);
                    if (gapDuration != null)
                    {
                        sendInfo.IsBlockedByGap = true;
                        sendInfo.WaitTimeDuration = gapDuration.Value;
                    }
                }
            }
        }

        private async Task CollectVerifyOtpInfo(OtpPipelineContext context)
        {
            var failedCountInfo = await _cacheFailedCount.GetAsync(context.CountFailedVerifyOtpCacheKey);
            if (failedCountInfo != null)
            {
                var verifyInfo = new VerifyInfo()
                {
                    VerifyFailCount = failedCountInfo.Count,
                };
                context.OtpSessionInfo.VerifyInfo = verifyInfo;
            }
        }
    }
}
