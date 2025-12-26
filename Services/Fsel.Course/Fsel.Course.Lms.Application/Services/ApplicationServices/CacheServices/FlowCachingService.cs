// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.FlowConfigs;

    public interface IFlowCachingService : IEntityCachingService<Flow>
    {
    }

    public class FlowCachingService : EntityCachingService<Flow>, IFlowCachingService
    {
        public FlowCachingService(ICacheService<Flow> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Flow),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Flow)}";
    }
}
