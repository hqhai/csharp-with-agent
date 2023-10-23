// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Microsoft.EntityFrameworkCore;

    public class FinalTestRepository : BaseRepository<FinalTest>, IFinalTestRepository
    {
        public FinalTestRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public override async Task<FinalTest?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(x => x.FinalTestSections.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x!.SectionQuestions.Where(n => !n.IsDeleted))
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
                return await Queryable.Include(x => x.FinalTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .ThenInclude(x => x!.Sections.Where(n => !n.IsDeleted))
                                      .ThenInclude(x => x!.SectionQuestions.Where(n => !n.IsDeleted))
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
                                          SectionGroups = x.FinalTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                                          {
                                              Id = x!.Id,
                                              ExecutionTime = x!.ExecutionTime,
                                              CourseSkill = x.CourseSkill,
                                              Sections = x.Sections.OrderBy(x => x!.DisplayOrder).Select(x => new SectionModel
                                              {
                                                  Id = x.Id,
                                                  Name = x.Name,
                                                  MediaPost = x.MediaPost,
                                                  VideoFilePath = x.VideoFilePath,
                                                  DisplayOrder = x.DisplayOrder,
                                                  TargetWord = x.TargetWord,
                                                  Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(x => new QuestionModel
                                                  {
                                                      Id = x!.Id,
                                                      QuestionType = x.QuestionType,
                                                      Explanation = x.Explanation,
                                                      Ungraded = x.Ungraded,
                                                      CorrectTotal = x.CorrectTotal,
                                                      Config = x.Config
                                                  }).ToList()
                                              }).ToList(),
                                          }).ToList(),
                                      }).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FinalTest?> GetAsync(Guid id, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.FinalTestSections.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x!.SectionQuestions.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x.FinalTestResults.Where(x => x.StudentId == studentId))
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
