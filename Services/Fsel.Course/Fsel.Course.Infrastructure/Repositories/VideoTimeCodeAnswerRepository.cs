// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeAnswerRepository : BaseRepository<VideoTimeCodeAnswer>, IVideoTimeCodeAnswerRepository
    {
        public VideoTimeCodeAnswerRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
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

        public async Task<VideoTimeCodeAnswer?> GetAsync(Guid videoTimeCodeId, Guid videoResultId, Guid? questionId, Guid? exerciseId)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.ExerciseId == exerciseId && x.VideoResultId == videoResultId && x.VideoTimeCodeId == videoTimeCodeId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
