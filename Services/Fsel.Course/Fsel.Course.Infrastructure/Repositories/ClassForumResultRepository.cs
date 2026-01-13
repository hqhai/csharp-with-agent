// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class ClassForumResultRepository : BaseRepository<ClassForumResult>, IClassForumResultRepository
    {
        public ClassForumResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public override async Task<ClassForumResult?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable
                .Include(x => x.LessonResult)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId)
        {
            var rows = await GetClassForumResultsAsync(courseId, studentId);

            return rows
                .Where(x => x.SkillScores != null && x.SkillScores.Any())
                .SelectMany(x => x.SkillScores!)
                .GroupBy(x => new { x.SkillId, x.Skill, x.SkillName })
                .Select(g => new SkillScores
                {
                    SkillId = g.Key.SkillId,
                    SkillFilePath = g.Where(x => x.SkillFilePath != null).FirstOrDefault()?.SkillFilePath,
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

        public async Task<IList<ClassForumResult>> GetClassForumResultsAsync(Guid courseId, Guid studentId)
        {
            return await ReadQueryable
                  .AsNoTracking()
                  .Where(x => x.LessonResult != null && x.LessonResult.StudentId == studentId && x.LessonResult.CourseId == courseId)
                  .ToListAsync();
        }
    }
}
