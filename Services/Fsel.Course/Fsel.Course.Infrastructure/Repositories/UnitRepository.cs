// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        private readonly IMapper _mapper;

        public UnitRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
            _mapper = mapper;
        }

        public override async Task<Unit?> GetIncludeByIdAsync(Guid id)
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

        public async Task<Unit?> GetIncludeAsync(Guid? id, Guid courseId, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.UnitSkillMockTests)
                                    .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                    .Include(x => x.UnitLessons)
                                    .Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
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

        public async Task<bool> IsUsingByClient(Guid id)
        {
            return await DbContext.Set<UnitResult>().AsQueryable()
                  .AnyAsync(x => x.UnitId == id);
        }
    }
}
