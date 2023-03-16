using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoRepository : BaseRepository<Video>, IVideoRepository
    {
        public VideoRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsVideoUsed(Guid Id)
        {
            return await Queryable
                 .Include(x => x.LessonVideos.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == Id && x.LessonVideos.Count > 0);
        }

        public override async Task<Video?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.VideoTimeCodes.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.TimeCodeExcercises.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Excercise)
                .ThenInclude(x => x.ExcerciseQuestions.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Question)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
