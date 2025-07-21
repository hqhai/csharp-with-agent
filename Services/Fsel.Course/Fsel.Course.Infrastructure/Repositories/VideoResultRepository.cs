// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoResultRepository : BaseRepository<VideoResult>, IVideoResultRepository
    {
        public VideoResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<VideoResult?> GetIncludeTimeCodeAnswerByIdAsync(Guid videoId, Guid lessonResultId, Guid studentId)
        {
            try
            {
                return await Queryable.Include(x => x.VideoTimeCodeResults).ThenInclude(x => x.VideoTimeCodeAnswers)
                                      .FirstOrDefaultAsync(x => x.VideoId == videoId && x.LessonResultId == lessonResultId && x.StudentId == studentId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
