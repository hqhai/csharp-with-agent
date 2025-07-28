// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Microsoft.Extensions.Localization;

    public class CheckBlockSendOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<CheckBlockSendOtpHandler>
    {
        private readonly ICacheService<SendOtpCountInfo> _sendOtpCountCache;
        private readonly IStringLocalizer _stringLocalizer;

        public CheckBlockSendOtpHandler(ICacheService<SendOtpCountInfo> sendOtpCountCache, IStringLocalizer stringLocalizer)
        {
            _sendOtpCountCache = sendOtpCountCache;
            _stringLocalizer = stringLocalizer;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            ArgumentNullException.ThrowIfNull(context.MinimumBetweenTwoSendsDuration, nameof(context.MinimumBetweenTwoSendsDuration));
            if (context.Step == OtpStep.SendOtp)
            {
                var sendCountInfo = await _sendOtpCountCache.GetAsync(context.CountSendOtpCacheKey);
                if (sendCountInfo != null)
                {
                    if (DateTime.UtcNow.Subtract(sendCountInfo.StartTime) > context.BlockSendOtpDuration)
                    {
                        await _sendOtpCountCache.RemoveAsync(context.CountSendOtpCacheKey);
                    }
                    else if (sendCountInfo.Count >= context.MaxCountOtpSend)
                    {
                        context.Status = false;
                        var remainingTime = context.BlockSendOtpDuration.Value - DateTime.UtcNow.Subtract(sendCountInfo.StartTime);

                        context.ErrorMessage = new KeyValuePair<string, string>(nameof(EnumAuthUserErrorCode.OtpTryResendAfterMinutes),
                        _stringLocalizer[nameof(EnumAuthUserErrorCode.OtpTryResendAfterMinutes)]
                            .Value.InjectParam(remainingTime.Minutes.ToString()));
                        return;
                    }
                    else if (DateTime.UtcNow.Subtract(sendCountInfo.LastSendTime) <= context.MinimumBetweenTwoSendsDuration.Value)
                    {
                        context.Status = false;
                        context.ErrorMessage = new KeyValuePair<string, string>(nameof(EnumAuthUserErrorCode.OtpTryResendAfterSeconds),
                        _stringLocalizer[nameof(EnumAuthUserErrorCode.OtpTryResendAfterSeconds)]
                            .Value.InjectParam(context.MinimumBetweenTwoSendsDuration.Value.Seconds.ToString()));
                        return;
                    }
                    context.OtpProviderType = OtpProviderType.Zalo;
                }
            }
            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
