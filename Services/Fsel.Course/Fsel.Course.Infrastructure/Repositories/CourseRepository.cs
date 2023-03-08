using Fsel.Core.Base;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseRepository : BaseRepository<EntityCourse>, ICourseRepository
    {
        public CourseRepository(CourseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnits.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnits.Count > 0);
        }

        public override async Task<EntityCourse?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.CourseUnits.Where(n => !n.IsDeleted)).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}