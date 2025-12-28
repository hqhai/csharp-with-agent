// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices.BuildModules
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;

    public interface ICourseBuildCachingService : IEntityCachingService<CourseBuildModel>
    {
    }

    public class CourseBuildCachingService : EntityCachingService<CourseBuildModel>, ICourseBuildCachingService
    {
        public CourseBuildCachingService(ICacheService<CourseBuildModel> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(CourseBuildModel),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(CourseBuildModel)}";
    }
}
