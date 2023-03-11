using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        public UnitRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnits.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnits.Count > 0);
        }
    }
}