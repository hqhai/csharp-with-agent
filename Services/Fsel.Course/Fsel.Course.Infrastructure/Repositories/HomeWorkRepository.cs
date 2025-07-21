// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace Fsel.Course.Infrastructure.Repositories
{
    public class HomeWorkRepository : BaseRepository<HomeWork>, IHomeWorkRepository
    {
        public HomeWorkRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsHomeWorkUsed(Guid? id)
        {
            return await Queryable
                 .Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == id && x.LessonHomeWorks.Count > 0);
        }

        public async Task<HomeWorkModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Where(x => x.Id == id)
                    .Select(x => new HomeWorkModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code,
                        MediaPost = x.MediaPost,
                        IsActive = x.LessonHomeWorks.Any(),
                        CourseLevel = x.CourseLevel,
                        CourseSkill = x.CourseSkill,
                        SkillId = x.SkillId,
                        SkillName = x.Skill != null ? x.Skill.Name : null,
                        Questions = x.HomeWorkQuestions.Where(m => m.Question != null && !m.IsDeleted).Select(m => m.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                        {
                            Id = m!.Id,
                            QuestionType = m.QuestionType,
                            Explanation = m.Explanation,
                            Ungraded = m.Ungraded,
                            CorrectTotal = m.CorrectTotal,
                            Config = m.Config
                        }).ToList()
                    }).AsNoTracking().FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<HomeWork>> GetListAsync(LessonResult lessonResult)
        {
            try
            {
                return await Queryable.Include(x => x!.LessonHomeWorks.Where(x => x.LessonId == lessonResult.LessonId))
                                        .Include(x => x!.HomeWorkQuestions)
                                        .Include(x => x.HomeWorkResults.Where(x => x.LessonResultId == lessonResult.Id))
                                        .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                        .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<HomeWork?> GetAsync(HomeWorkResult homeWorkResult)
        {
            try
            {
                return await Queryable
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.Question)
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.HomeWorkAnswers.Where(n => n.HomeWorkResultId == homeWorkResult.Id))
                        .Where(x => x.Id == homeWorkResult.HomeWorkId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUsingByClient(Guid id)
        {
            return await DbContext.Set<HomeWorkResult>().AsQueryable()
                  .AnyAsync(x => x.HomeWorkId == id);
        }
    }
}
