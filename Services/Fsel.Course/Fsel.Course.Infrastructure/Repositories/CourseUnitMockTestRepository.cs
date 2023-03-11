using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseUnitMockTestRepository : BaseRepository<CourseUnitMockTest>, ICourseUnitMockTestRepository
    {
        public CourseUnitMockTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<CourseUnitMockTest>> GetListByUnitIdAsync(Guid courseId)
        {
            return await Queryable.Where(a => a.CourseId == courseId).ToListAsync();
        }
    }
}