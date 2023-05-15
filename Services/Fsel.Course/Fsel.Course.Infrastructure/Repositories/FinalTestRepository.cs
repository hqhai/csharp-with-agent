// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Microsoft.EntityFrameworkCore;

    public class FinalTestRepository : BaseRepository<FinalTest>, IFinalTestRepository
    {
        public FinalTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<FinalTest?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(x => x.FinalTestExercises)
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FinalTestModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.FinalTestExercises.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.Exercise)
                                      .ThenInclude(x => x.ExerciseQuestions)
                                      .ThenInclude(x => x.Question)
                                      .Where(x => x.Id == id)
                                      .Select(x => new FinalTestModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          IsActive = x.IsActive,
                                          FinalTestLevel = x.FinalTestLevel,
                                          CreatedDate = x.CreatedDate,
                                          CreatedFullName = x.CreatedFullName,
                                          ExecutionTime = x.ExecutionTime,
                                          Exercises = x.FinalTestExercises.Select(x => x.Exercise).Select(x => new ExerciseModel
                                          {
                                              Id = x!.Id,
                                              Name = x.Name,
                                              CourseSkill = x.CourseSkill,
                                              CreatedDate = x.CreatedDate,
                                              CreatedFullName = x.CreatedFullName,
                                              MediaPost = x.MediaPost,
                                              Questions = x.ExerciseQuestions.Select(x => x.Question).Select(x => new QuestionModel
                                              {
                                                  Id = x!.Id,
                                                  QuestionType = x.QuestionType,
                                                  Config = x.Config,
                                                  CorrectTotal = x.CorrectTotal,
                                                  CreatedDate = x.CreatedDate,
                                                  CreatedFullName = x.CreatedFullName,
                                                  Explanation = x.Explanation,
                                                  Ungraded = x.Ungraded,
                                              }).ToList(),
                                          }).ToList(),
                                      }).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
