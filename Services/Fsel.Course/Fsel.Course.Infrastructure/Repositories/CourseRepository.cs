// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseRepository : BaseRepository<EntityCourse>, ICourseRepository
    {
        public CourseRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }

        public override async Task<EntityCourse?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Unit)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.MockTest)
                .Include(x => x.CourseTeachers.Where(c => !c.IsDeleted)).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.CourseResult)
                                        .Include(e => e.CourseClassStudents.Where(y => y.IsDeleted == false))
                                        .Include(x => x.CourseUnitMockTests.Where(y => y.IsDeleted == false))
                                        .ThenInclude(x => x.Unit)
                                        .ThenInclude(x => x!.UnitLessons.Where(y => y.IsDeleted == false))
                                        .ThenInclude(x => x.Lesson)
                                        .ThenInclude(x => x!.LessonVideos.Where(y => y.IsDeleted == false))
                                        .Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
