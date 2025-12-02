// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface ILessonModuleCachingService : IEntityCachingService<LessonModule>
    {
    }

    public class LessonModuleCachingService : EntityCachingService<LessonModule>, ILessonModuleCachingService
    {
        public LessonModuleCachingService(ICacheService<LessonModule> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(LessonModule),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(LessonModule)}";
    }
}
