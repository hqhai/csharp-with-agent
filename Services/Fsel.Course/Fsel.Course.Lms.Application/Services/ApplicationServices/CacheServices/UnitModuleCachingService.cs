// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface IUnitModuleCachingService : IEntityCachingService<UnitModule>
    {
    }

    public class UnitModuleCachingService : EntityCachingService<UnitModule>, IUnitModuleCachingService
    {
        public UnitModuleCachingService(ICacheService<UnitModule> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(UnitModule),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(UnitModule)}";
    }
}
