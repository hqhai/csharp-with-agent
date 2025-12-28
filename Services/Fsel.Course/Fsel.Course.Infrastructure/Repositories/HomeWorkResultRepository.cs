// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class HomeWorkResultRepository : BaseRepository<HomeWorkResult>, IHomeWorkResultRepository
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public HomeWorkResultRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<bool> GetCheckByIdsAsync(IEnumerable<Guid>? ids)
        {
            try
            {
                return await Queryable.AnyAsync(e => ids != null && ids.Contains(e.Id));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<HomeWorkResult>> GetHomeWorkResultsAsync(Guid courseId, Guid studentId)
        {
            return await (from lr in _lessonResultRepository.ReadQueryable.AsNoTracking()
                          join vr in ReadQueryable.Include(x => x.LessonModule) on lr.Id equals vr.LessonResultId
                          where lr.CourseId == courseId && lr.StudentId == studentId
                          select vr).ToListAsync();
        }

        public async Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId)
        {
            var homeWorkResults = await GetHomeWorkResultsAsync(courseId, studentId);

            return homeWorkResults
                .Where(x => x.SkillScores != null && x.SkillScores.Any())
                .SelectMany(x => x.SkillScores!)
                .GroupBy(x => new { x.SkillId, x.Skill, x.SkillName })
                .Select(g => new SkillScores
                {
                    SkillId = g.Key.SkillId,
                    Skill = g.Key.Skill,
                    SkillName = g.Key.SkillName,

                    CorrectCount = g.Sum(s => s.CorrectCount),
                    CorrectQuestion = g.Sum(s => s.CorrectQuestion ?? 0),
                    CountQuestion = g.Sum(s => s.CountQuestion),

                    Percent = g.Average(s => s.Percent),
                    TokenReceived = g.Sum(s => s.TokenReceived),

                    TotalQuestion = g.Sum(s => s.TotalQuestion),
                    TotalCount = g.Sum(s => s.TotalCount)
                })
                .ToList();
        }
    }
}
