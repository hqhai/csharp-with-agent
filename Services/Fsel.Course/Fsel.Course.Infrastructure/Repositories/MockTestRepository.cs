// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Microsoft.EntityFrameworkCore;

    public class MockTestRepository : BaseRepository<MockTest>, IMockTestRepository
    {
        private readonly SectionConverter _sectionConverter;
        private readonly IMapper _mapper;

        public MockTestRepository(CourseDbContext dbContext, AuthContext authContext, SectionConverter sectionConverter, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _sectionConverter = sectionConverter;
            _mapper = mapper;
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
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionTimeCodes.Where(y => !y.IsDeleted))
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionParts.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                       .ThenInclude(x => x.Question)
                                       .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                                       .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                                       .OrderBy(x => x!.CreatedDate)
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
                return await Queryable.Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionTimeCodes.Where(y => !y.IsDeleted))
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionParts.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                       .ThenInclude(x => x.Question)
                                       .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                                       .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                                       .OrderBy(x => x!.CreatedDate)
                                       .Where(x => x.Id == id)
                                       .AsNoTracking()
                                       .Select(x => new MockTestModel
                                       {
                                           Id = x.Id,
                                           Name = x.Name,
                                           CreatedDate = x.CreatedDate,
                                           IsActive = x.UnitSkillMockTests.Any() || x.CourseUnitMockTests.Any(),
                                           MockTestType = x.MockTestType,
                                           SectionGroups = x.MockTestSections.Where(x => x.SectionGroup != null)
                                             .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
                                             .Select(x => _sectionConverter.GetSectionGroupModel(x, false)).ToList(),
                                       }).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MockTest?> GetAsync(Guid mockTestId, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionTimeCodes.Where(y => !y.IsDeleted))
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionParts.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                       .ThenInclude(x => x.Question)
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                       .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                       .ThenInclude(x => x.SectionGroup).ThenInclude(x => x!.SectionGroupResults.Where(x => x.StudentId == studentId))
                                       .Where(x => x.Id == mockTestId)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
