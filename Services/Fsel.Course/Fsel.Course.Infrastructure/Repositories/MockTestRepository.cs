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

    public class MockTestRepository : BaseRepository<MockTest>, IMockTestRepository
    {
        public MockTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsUnitSkillMockTest(Guid id)
        {
            return await Queryable
                .Include(x => x.UnitSkillMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.UnitSkillMockTests.Count > 0);
        }

        public async Task<bool> IsCourseFullMockTest(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.UnitSkillMockTests.Count > 0);
        }

        public override async Task<MockTest?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections)
                                       .ThenInclude(x => x.SectionTimeCodes)
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections)
                                       .ThenInclude(x => x.SectionParts)
                                       .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                       .ThenInclude(x => x.Question)
                                       .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MockTestModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .ThenInclude(x => x!.Sections)
                                      .ThenInclude(x => x.SectionParts)
                                      .ThenInclude(x => x.SectionQuestions)
                                      .Where(x => x.Id == id)
                                      .Select(x => new MockTestModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          CourseType = x.CourseType,
                                          CreatedDate = x.CreatedDate,
                                          IsActive = x.IsActive,
                                          MockTestType = x.MockTestType,
                                          SectionGroups = x.MockTestSections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                          {
                                              Id = x!.Id,
                                              ExecutionTime = x.ExecutionTime,
                                              CourseSkill = x.CourseSkill,
                                              Sections = x.Sections.Select(x => new SectionModel
                                              {
                                                  Id = x.Id,
                                                  Name = x.Name,
                                                  MediaPost = x.MediaPost,
                                                  VideoFilePath = x.VideoFilePath,
                                                  DisplayOrder = x.DisplayOrder,
                                                  TargetWord = x.TargetWord,
                                                  SectionTimeCodes = x.SectionTimeCodes.Select(x => new SectionTimeCodeModel
                                                  {
                                                      Id = x.Id,
                                                      Name = x.Name,
                                                      DisplayTime = x.DisplayTime,
                                                      ExecutionTime = x.ExecutionTime,
                                                      SectionId = x.SectionId,
                                                  }).ToList(),
                                                  SectionParts = x.SectionParts.Select(x => new SectionPartModel
                                                  {
                                                      Id = x.Id,
                                                      PartName = x.PartName,
                                                      SectionId = x.SectionId,
                                                      Questions = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
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
