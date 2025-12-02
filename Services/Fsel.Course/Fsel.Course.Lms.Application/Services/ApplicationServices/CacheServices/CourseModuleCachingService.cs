// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface ICourseModuleCachingService : IEntityCachingService<CourseModule>
    {
    }

    public class CourseModuleCachingService : EntityCachingService<CourseModule>, ICourseModuleCachingService
    {
        public CourseModuleCachingService(ICacheService<CourseModule> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(CourseModule),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(CourseModule)}";
    }
}
