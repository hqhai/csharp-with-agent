// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class MockTestRepository : BaseRepository<MockTest>, IMockTestRepository
    {
        public MockTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsUnitSkillMockTest(Guid id)
        {
            return await Queryable
                .Include(x => x.UnitSkillMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.UnitSkillMockTests.Count > 0);
        }

        public async Task<bool> IsCourseUnitMockTest(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.UnitSkillMockTests.Count > 0);
        }
    }
}
