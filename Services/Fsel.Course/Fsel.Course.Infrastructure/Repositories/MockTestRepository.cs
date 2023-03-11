using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class MockTestRepository : BaseRepository<MockTest>, IMockTestRepository
    {
        public MockTestRepository(BaseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsUnitSkillMockTest(Guid Id)
        {
            return await Queryable
                .Include(x => x.UnitSkillMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == Id && x.UnitSkillMockTests.Count > 0);
        }
    }
}