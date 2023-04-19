// Copyright (c) Atlantic. All rights reserved.

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

        public async Task<VideoTimeCodeAnswer?> GetAsync(Guid videoResultId, Guid questionId, Guid? exerciseId, Guid? videoTimeCodeId)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.ExerciseId == exerciseId && x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResultId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
