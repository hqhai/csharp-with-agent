// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using static Fsel.Course.Lms.Application.Queries.TestQuery.GetTestSectionResultDetailQueryHandler;

    public interface ITestSectionCachingService : IEntityCachingService<CachedSectionTreeModel>
    {
    }

    public class TestSectionCachingService : EntityCachingService<CachedSectionTreeModel>, ITestSectionCachingService
    {
        public TestSectionCachingService(ICacheService<CachedSectionTreeModel> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(TestSection),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(TestSection)}";
    }
}
