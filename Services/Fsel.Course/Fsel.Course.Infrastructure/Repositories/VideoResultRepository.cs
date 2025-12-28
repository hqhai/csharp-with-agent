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
        private readonly ILessonResultRepository _lessonResultRepository;

        public VideoResultRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _lessonResultRepository = lessonResultRepository;
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

        public async Task<List<VideoResult>> GetVideoResultsAsync(Guid courseId, Guid studentId)
        {
            return await (from lr in _lessonResultRepository.ReadQueryable.AsNoTracking()
                          join vr in ReadQueryable.Include(x => x.LessonModule) on lr.Id equals vr.LessonResultId
                          where lr.CourseId == courseId && lr.StudentId == studentId
                          select vr).ToListAsync();
        }
    }
}
