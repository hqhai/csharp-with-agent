// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using Fsel.Common.Caching;
    using Fsel.Core.Localization;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Microsoft.Extensions.Localization;

    public class VerifyOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<VerifyOtpHandler>
    {
        private readonly ICacheService<string> _cache;
        private readonly IStringLocalizer _stringLocalizer;

        public VerifyOtpHandler(ICacheService<string> cache, IStringLocalizer stringLocalizer)
        {
            _cache = cache;
            _stringLocalizer = stringLocalizer;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            if (context.Step == OtpStep.VerifyOtp)
            {
                var value = await _cache.GetAsync(context.OtpCacheKey);
                if (value == null)
                {
                    context.Status = false;
                    context.ErrorMessage = new KeyValuePair<string, string>(nameof(EnumAuthUserErrorCode.OTPExpired),
                        _stringLocalizer[nameof(EnumAuthUserErrorCode.OTPExpired)]);

                    return;
                }
                else
                {
                    context.Status = value.Equals(context.RequestOtp, StringComparison.OrdinalIgnoreCase);
                    context.Otp = value;
                }
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
