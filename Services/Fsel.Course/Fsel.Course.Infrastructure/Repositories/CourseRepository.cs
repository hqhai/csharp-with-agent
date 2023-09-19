// Copyright (c) Atlantic. All rights reserved.

using System.Threading;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
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
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.Unit)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.FinalTest)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.MockTest)
                .Include(x => x.CourseTeachers.Where(c => !c.IsDeleted).OrderBy(x => x.CreatedDate))
                .FirstOrDefaultAsync(x => x.Id == id);
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
                return await Queryable.Include(x => x.CourseResults)
                                        .Include(x => x.CourseUnitMockTests.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Unit)
                                        .ThenInclude(x => x!.UnitLessons.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Lesson)
                                        .ThenInclude(x => x!.LessonVideos.Where(y => y.IsDeleted == false).OrderBy(x => x.CreatedDate))
                                        .Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeCourseResult(Guid id, Guid? studentId)
        {
            return await Queryable
                         .Include(x => x.CourseResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.Unit)
                         .ThenInclude(x => x!.UnitResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.MockTest)
                         .ThenInclude(x => x!.MockTestResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.FinalTest)
                         .ThenInclude(x => x!.FinalTestResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseTeachers)
                         .Where(x => x.Id == id)
                         .AsNoTracking()
                         .FirstOrDefaultAsync();
        }
    }
}
