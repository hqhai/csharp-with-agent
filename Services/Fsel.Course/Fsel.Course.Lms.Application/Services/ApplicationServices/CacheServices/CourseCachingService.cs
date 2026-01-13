// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Course = Domain.Entities.Course;

    public interface ICourseCachingService : IEntityCachingService<Course>
    {
        Task<IList<Course>> GetAllAvailableCoursesAsync();
    }

    public class CourseCachingService : EntityCachingService<Course>, ICourseCachingService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseCachingService(ICacheService<Course> cacheService, ICourseRepository courseRepository) : base(cacheService)
        {
            _courseRepository = courseRepository;
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Course),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Course)}";

        public async Task<IList<Course>> GetAllAvailableCoursesAsync()
        {
            var courses = await GetOrSetAsync("All", async (ctx, _) =>
            {
                return await _courseRepository.ReadQueryable.Where(x => x.Status == EnumCourseStatus.Active
                && x.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(_);
            });
            return courses ?? new List<Course>();
        }
    }
}
