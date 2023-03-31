using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeAnswerRepository : BaseRepository<VideoTimeCodeAnswer>, IVideoTimeCodeAnswerRepository
    {
        public VideoTimeCodeAnswerRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<VideoTimeCodeAnswer?> GetWhereByIdAsync(Guid videoResultId, Guid questionId, Guid? exerciseId, Guid? videoTimeCodeId)
        {
            try
            {
                return await Queryable.Where(x => x.QuestionId == questionId && x.ExerciseId == exerciseId)
                                      .Where(x => x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResultId)
                                      .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
