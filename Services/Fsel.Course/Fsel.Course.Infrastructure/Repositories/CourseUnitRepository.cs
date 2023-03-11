using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseUnitRepository : BaseRepository<CourseUnit>, ICourseUnitRepository
    {
        public CourseUnitRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<CourseUnit>> GetListByUnitIdAsync(Guid courseId)
        {
            return await Queryable.Where(a => a.CourseId == courseId).ToListAsync();
        }
    }
}