using Fsel.Core.Base;
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
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted)).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}