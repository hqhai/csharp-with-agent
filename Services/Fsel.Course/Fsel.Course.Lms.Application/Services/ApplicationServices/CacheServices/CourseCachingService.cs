// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Course = Domain.Entities.Course;

    public interface ICourseCachingService : IEntityCachingService<Course>
    {
    }

    public class CourseCachingService : EntityCachingService<Course>, ICourseCachingService
    {
        public CourseCachingService(ICacheService<Course> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Course),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Course)}";
    }
}
