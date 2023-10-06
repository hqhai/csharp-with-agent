// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        public UnitRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<Unit?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.UnitSkillMockTests.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.MockTest)
                .ThenInclude(x => x!.MockTestSections.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.SectionGroup)
                .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Lesson)
                .ThenInclude(x => x!.LessonInstructions.Where(n => !n.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Unit>?> GetListAsync(IList<Guid>? ids, Guid? studentId)
        {
            if (ids == null || !ids.Any())
            {
                return null;
            }
            return await Queryable.Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons).Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<List<Unit>?> GetListAsync(Guid? studentId, Guid courseId, EnumProcessType type)
        {
            var query = Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                               .Include(x => x.CourseUnitMockTests)
                               .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == courseId));
            if (type == EnumProcessType.LessonVideo)
            {
                return await query.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson)
                               .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                               .AsNoTracking()
                               .ToListAsync();
            }
            else if (type == EnumProcessType.HomeWork)
            {
                return await query.Include(x => x.UnitLessons)
                                    .ThenInclude(x => x.Lesson)
                                    .ThenInclude(x => x!.LessonHomeWorks)
                                    .ThenInclude(x => x.HomeWork)
                                .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                                .ThenInclude(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == courseId))
                                .AsNoTracking()
                                .ToListAsync();
            }
            else if (type == EnumProcessType.ClassForum)
            {
                return await query.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson).ThenInclude(x => x!.ClassForum)
                              .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                              .ThenInclude(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                              .AsNoTracking()
                              .ToListAsync();
            }
            else
            {
                return await query.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                               .Include(x => x.UnitLessons)
                               .ThenInclude(x => x.Lesson)
                               .ThenInclude(x => x!.LessonVideos)
                               .AsNoTracking()
                               .ToListAsync();
            }
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }
    }
}
