using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<Lesson?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsLessonUsed(Guid id)
        {
            return await Queryable
                 .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == id && x.UnitLessons.Count > 0);
        }

        public async Task<Lesson?> GetIncludeVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.LessonResults)
                                      .Include(x => x.LessonVideos)
                                      .ThenInclude(x => x.Video)
                                      .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
