// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<Question>?> GetIncludeTimeCodeByIdAsync(IEnumerable<Guid> ids)
        {
            try
            {
                return await Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.TimeCodeExercises)
                                    .ThenInclude(x => x.VideoTimeCode)
                                    .Where(x => ids.Contains(x.Id)).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Question>?> GetIncludeSectionByIdAsync(IEnumerable<Guid> ids)
        {
            try
            {
                return await Queryable.Include(x => x.SectionQuestions)
                                    .Where(x => ids.Contains(x.Id)).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
