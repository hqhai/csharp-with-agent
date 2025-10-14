// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities;

    public interface ICategoryCachingService : IEntityCachingService<Category>
    {
    }

    public class CategoryCachingService : EntityCachingService<Category>, ICategoryCachingService
    {
        public CategoryCachingService(ICacheService<Category> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Category),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Category)}";
    }
}
