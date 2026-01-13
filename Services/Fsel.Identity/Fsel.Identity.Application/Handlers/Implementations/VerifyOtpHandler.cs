// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;

    public class VerifyOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<VerifyOtpHandler>
    {
        private readonly ICacheService<string> _cache;

        public VerifyOtpHandler(ICacheService<string> cache)
        {
            _cache = cache;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            var value = await _cache.GetAsync(context.OtpCacheKey);
            if (value == null)
            {
                context.Status = false;
                return;
            }
            else
            {
                context.Status = value.Equals(context.RequestOtp, StringComparison.OrdinalIgnoreCase);
                context.Otp = value;
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
