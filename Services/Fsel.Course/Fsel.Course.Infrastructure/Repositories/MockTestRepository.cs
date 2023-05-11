// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
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

        public async Task<bool> IsCourseUnitMockTest(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.UnitSkillMockTests.Count > 0);
        }

        public async Task<MockTestModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .ThenInclude(x => x.Sections)
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
                                          MockTestSections = x.MockTestSections.Where(x => !x.IsDeleted).Select(x => new MockTestSectionModel
                                          {
                                              Id = x.Id,
                                              SectionGroups = x.SectionGroup!.Sections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                              {
                                                  ExecutionTime = x.ExecutionTime,
                                                  CourseSkill = x.CourseSkill,
                                                  Sections = x.Sections.Select(x => new SectionModel
                                                  {
                                                      Id = x.Id,
                                                      Name = x.Name,
                                                      MediaPost = x.MediaPost,
                                                      TargetWord = x.TargetWord,
                                                      CreatedDate = x.CreatedDate,
                                                      CreatedUserId = x.CreatedUserId,
                                                      SectionParts = x.SectionParts.Select(x => new SectionPartModel
                                                      {
                                                          Id = x.Id,
                                                          CreatedDate = x.CreatedDate,
                                                          PartName = x.PartName,
                                                          SectionId = x.SectionId,
                                                          CreatedFullName = x.CreatedFullName,
                                                          SectionQuestions = x.SectionQuestions.Select(x => new SectionQuestionModel
                                                          {
                                                              Questions = x.Question!.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                                                              {
                                                                  Id = x!.Id,
                                                                  QuestionType = x.QuestionType,
                                                                  Explanation = x.Explanation,
                                                                  Ungraded = x.Ungraded,
                                                                  CorrectTotal = x.CorrectTotal,
                                                                  Config = x.Config
                                                              }).ToList(),
                                                          }).ToList(),
                                                      }).ToList(),
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

        public IQueryable<MockTestModel> SearchAsync(EnumMockTestType? mockTestType)
        {
            try
            {
                return Queryable.Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .ThenInclude(x => x.Sections)
                                      .ThenInclude(x => x.SectionParts)
                                      .ThenInclude(x => x.SectionQuestions)
                                      .Where(x => mockTestType == EnumMockTestType.SkillMockTest)
                                      .Select(x => new MockTestModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          CourseType = x.CourseType,
                                          CreatedDate = x.CreatedDate,
                                          IsActive = x.IsActive,
                                          MockTestType = x.MockTestType,
                                          MockTestSections = x.MockTestSections.Where(x => !x.IsDeleted).Select(x => new MockTestSectionModel
                                          {
                                              SectionGroups = x.SectionGroup!.Sections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                              {
                                                  CourseSkill = x.CourseSkill
                                              }).ToList(),
                                          }).ToList(),
                                      });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
