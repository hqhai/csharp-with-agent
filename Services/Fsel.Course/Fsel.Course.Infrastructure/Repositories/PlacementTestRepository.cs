using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class PlacementTestRepository : BaseRepository<PlacementTest>, IPlacementTestRepository
    {
        public PlacementTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}