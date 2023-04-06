// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class HomeWorkRepository : BaseRepository<HomeWork>, IHomeWorkRepository
    {
        public HomeWorkRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
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
                return await Queryable.Include(x => x.HomeWorkQuestions)
                    .ThenInclude(x => x.Question)
                    .Where(x => x.Id == id)
                    .Select(x => new HomeWorkModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code,
                        MediaPost = x.MediaPost,
                        IsActive = x.LessonHomeWorks.Any(),
                        CourseLevel = x.CourseLevel,
                        CourseSkill = x.CourseSkill,
                        Questions = x.HomeWorkQuestions.Where(m => m.Question != null).Select(m => m.Question).Select(m => new QuestionModel()
                        {
                            Id = m!.Id,
                            QuestionType = m.QuestionType,
                            Explanation = m.Explanation,
                            Ungraded = m.Ungraded,
                            CorrectTotal = m.CorrectTotal,
                            Config = m.Config
                        }).ToList()
                    }).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
