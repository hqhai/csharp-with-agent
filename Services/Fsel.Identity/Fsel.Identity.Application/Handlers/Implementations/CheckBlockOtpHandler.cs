// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Shared.Helpers;
    using Microsoft.Extensions.Localization;

    public class CheckBlockOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<CheckBlockOtpHandler>
    {
        private readonly ICacheService<string> _cache;
        private readonly IStringLocalizer _stringLocalizer;

        public CheckBlockOtpHandler(ICacheService<string> cache, IStringLocalizer stringLocalizer)
        {
            _cache = cache;
            _stringLocalizer = stringLocalizer;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            var value = await _cache.GetAsync(context.BlockedOtpCacheKey);
            if (value != null)
            {
                var blockToTime = DateTime.Parse(value);
                if (blockToTime > DateTime.UtcNow)
                {
                    context.Status = false;
                    context.ErrorMessage = _stringLocalizer["i18n_OTP_block_in_minutes"].Value.InjectParam(blockToTime.Subtract(DateTime.UtcNow).Minutes.ToString());
                    return;
                }
                else
                {
                    // If the block time has expired, remove the cache entry
                    await _cache.RemoveAsync(context.BlockedOtpCacheKey);
                }
            }
            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
