// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.ApplicationServices.CacheServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.Caching;

    public interface IRequestSafeCachingService : IEntityCachingService<string>
    {
        Task<(bool, T)> SafeRequest<T>(string key, Func<Task<T>> safeFunction);
    }

    public class RequestSafeCachingService : EntityCachingService<string>, IRequestSafeCachingService
    {
        public RequestSafeCachingService(ICacheService<string> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string> { "SafeRequest", "Entity" };

        public override string Prefix => $"SafeRequest";

        public async Task<(bool, T)> SafeRequest<T>(string key, Func<Task<T>> safeFunction)
        {
            var hadSetCache = false;
            var result = await GetOrSetAsync(key, async (ctx, _) =>
            {
                hadSetCache = true;
                ctx.Options.Duration = TimeSpan.FromSeconds(5);
                return "Process";
            });

            if (hadSetCache)
            {
                try
                {
                    if (safeFunction != null)
                    {
                        var functionResult = await safeFunction();
                        return (true, functionResult);
                    }
                    else
                    {
                        return (true, default);
                    }
                }
                finally
                {
                    await ClearCache(key);
                }
            }
            else
            {
                return (false, default(T));
            }
        }
    }
}
