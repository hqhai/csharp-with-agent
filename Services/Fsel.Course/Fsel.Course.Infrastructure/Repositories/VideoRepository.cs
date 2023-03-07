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
    public class VideoRepository : BaseRepository<Video>, IVideoRepository
    {
        public VideoRepository(CourseDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<bool> IsVideoLesson(Guid Id)
        {
            return await Queryable
                 .Include(x => x.lessonVideos.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == Id && x.lessonVideos.Count > 0);
        }
    }
}
