// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        public UnitRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
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

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }
    }
}
