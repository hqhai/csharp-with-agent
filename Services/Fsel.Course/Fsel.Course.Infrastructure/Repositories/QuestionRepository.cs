// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<List<Question>?> GetListAsync(IEnumerable<Guid> ids)
        {
            try
            {
                return await Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.TimeCodeExercises)
                                    .WhereBulkContains(ids, x => x.Id).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Question>?> GetIncludeSectionByIdAsync(IEnumerable<Guid> ids, double? version = null)
        {
            try
            {
                if (version == (int)EnumVersion.V1)
                {
                    return await Queryable.Include(x => x.SectionQuestions)
                                      .ThenInclude(x => x.SectionPart)
                                      .ThenInclude(x => x.Section)
                                      .WhereBulkContains(ids, x => x.Id).ToListAsync();
                }
                return await Queryable.Include(x => x.SectionQuestions)
                                      .ThenInclude(x => x.Section)
                                      .WhereBulkContains(ids, x => x.Id).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Question>> GetIncludeByHomeWorkAsync(IEnumerable<Guid> ids)
        {
            try
            {
                return await Queryable.Include(x => x.HomeWorkQuestions)
                                    .WhereBulkContains(ids, x => x.Id).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Question>?> GetIncludeTimeCodeByIdAsync(IEnumerable<Guid> ids)
        {
            try
            {
                return await Queryable.Include(x => x.ExerciseQuestions)
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x.TimeCodeExercises)
                                    .ThenInclude(x => x.VideoTimeCode)
                                    .WhereBulkContains(ids, x => x.Id).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
