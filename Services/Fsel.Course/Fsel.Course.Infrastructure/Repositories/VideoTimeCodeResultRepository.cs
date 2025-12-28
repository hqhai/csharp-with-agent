// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeResultRepository : BaseRepository<VideoTimeCodeResult>, IVideoTimeCodeResultRepository
    {
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;

        public VideoTimeCodeResultRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
        }

        public async Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId, EnumTimeCodeType timeCodeType)
        {
            var queryResult = from baseQ in ReadQueryable
                              join vtc in _videoTimeCodeRepository.ReadQueryable on baseQ.VideoTimeCodeId equals vtc.Id
                              join vr in _videoResultRepository.ReadQueryable on baseQ.VideoResultId equals vr.Id
                              where vr.LessonResult != null && vr.LessonResult.StudentId == studentId
                                    && vr.LessonResult.CourseId == courseId
                                    && vtc.TimeCodeType == timeCodeType
                              select baseQ;
            var videoTimeCodeResults = await queryResult.ToListAsync();
            return videoTimeCodeResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!)
                .GroupBy(x => new { x.SkillId, x.Skill, x.SkillName })
                .Select(x => new SkillScores
                {
                    SkillId = x.Key.SkillId,
                    Skill = x.Key.Skill,
                    SkillName = x.Key.SkillName,
                    CorrectCount = x.Sum(s => s.CorrectCount),
                    CorrectQuestion = x.Sum(s => s.CorrectQuestion ?? 0),
                    CountQuestion = x.Sum(s => s.CountQuestion),
                    Percent = x.Average(s => s.Percent),
                    TokenReceived = x.Sum(s => s.TokenReceived),
                    TotalQuestion = x.Sum(s => s.TotalQuestion),
                    TotalCount = x.Sum(s => s.TotalCount)
                }).ToList();
        }
    }
}
