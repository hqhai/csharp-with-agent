// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeAnswerRepository : BaseRepository<VideoTimeCodeAnswer>, IVideoTimeCodeAnswerRepository
    {
        public VideoTimeCodeAnswerRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<VideoTimeCodeAnswer?> GetAsync(Guid videoTimeCodeResultId, Guid questionId, Guid? exerciseId)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.ExerciseId == exerciseId && x.VideoTimeCodeResultId == videoTimeCodeResultId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
