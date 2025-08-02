// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;

    public class CheckBlockOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<CheckBlockOtpHandler>
    {
        private readonly ICacheService<string> _cache;
        private ICacheService<FailedCountInfo> _cacheFailedCount;

        public CheckBlockOtpHandler(ICacheService<string> cache, ICacheService<FailedCountInfo> cacheFailedCount)
        {
            _cache = cache;
            _cacheFailedCount = cacheFailedCount;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            var value = await _cache.GetAsync(context.BlockedOtpCacheKey);
            if (value != null)
            {
                if (!DateTime.TryParse(value, out var blockToTime)
                    || blockToTime > DateTime.UtcNow)
                {
                    context.Status = false;
                    return;
                }

                await _cache.RemoveAsync(context.BlockedOtpCacheKey);
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
