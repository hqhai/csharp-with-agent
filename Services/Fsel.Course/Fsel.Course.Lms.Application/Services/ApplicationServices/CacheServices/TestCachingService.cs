// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public interface ITestCachingService : IEntityCachingService<Test>
    {
    }

    public class TestCachingService : EntityCachingService<Test>, ITestCachingService
    {
        public TestCachingService(ICacheService<Test> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Test),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Test)}";
    }
}
