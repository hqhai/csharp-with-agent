// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class TestRepository : BaseRepository<Test>, ITestRepository
    {
        public TestRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsUsingByClient(Guid originalId)
        {
            var categoryCheck = await DbContext.Set<CategoryTestBank>().AsQueryable().AnyAsync(x => x.TestOriginalId == originalId);
            var unitCheck = await DbContext.Set<UnitModule>().AsQueryable().AnyAsync(x => x.OriginalId == originalId);
            var courseCheck = await DbContext.Set<CourseModule>().AsQueryable().AnyAsync(x => x.OriginalId == originalId);
            return categoryCheck || unitCheck || courseCheck;
        }

        public async Task<List<Guid>> GetUsedOriginalIdsAsync(IList<Guid> originalIds)
        {
            var categoryResults = await DbContext.Set<CategoryTestBank>().AsQueryable().WhereBulkContains(originalIds, x => x.TestOriginalId)
                .Select(x => x.TestOriginalId)
                .ToListAsync();

            var unitResults = await DbContext.Set<UnitModule>().AsQueryable().WhereBulkContains(originalIds, x => x.OriginalId)
                .Select(x => x.OriginalId)
                .ToListAsync();

            var courseResults = await DbContext.Set<CourseModule>().AsQueryable().WhereBulkContains(originalIds, x => x.OriginalId)
                .Select(x => x.OriginalId)
                .ToListAsync();

            var originalUsedIds = categoryResults
                                .Concat(unitResults)
                                .Concat(courseResults)
                                .Distinct()
                                .ToList();
            return originalUsedIds;
        }

        public async Task<Test> GetTestAsync(Test test)
        {
            ArgumentNullException.ThrowIfNull(test);

            var allSections = await DbContext.Set<TestSection>().AsQueryable()
                                             .Where(x => x.TestId == test.Id)
                                             .ToListAsync();

            var allTestSectionQuestions = await DbContext.Set<TestSectionQuestion>().AsQueryable()
                                                         .WhereBulkContains(allSections.Select(x => x.Id), x => x.TestSectionId)
                                                         .Include(x => x.Question).ToListAsync();
            var testSectionQuestionDict = allTestSectionQuestions.GroupBy(x => x.TestSectionId)
                                   .ToDictionary(
                                       g => g.Key,
                                       g => g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                                   );
            var allTestAISettings = await DbContext.Set<TestAISetting>().AsQueryable().Include(x => x.TestAICriteriaSettings)
                                                   .WhereBulkContains(allSections.Select(x => x.Id), x => x.TestSectionId)
                                                   .ToListAsync();
            var testAISettingDict = allTestAISettings.GroupBy(x => x.TestSectionId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                        );

            var rootSections = allSections
                     .Where(x => !x.ParentId.HasValue)
                     .ToList();

            foreach (var root in rootSections)
            {
                BuildSectionTreeRecursive(root, allSections, testSectionQuestionDict, testAISettingDict);
            }
            test.TestSections = rootSections;
            return test;
        }

        private void BuildSectionTreeRecursive(TestSection parent, List<TestSection> allSections, Dictionary<Guid, List<TestSectionQuestion>> testSectionQuestionDict, Dictionary<Guid, List<TestAISetting>> testAISettingDict)
        {
            var children = allSections.Where(x => x.ParentId == parent.Id).OrderBy(x => x.DisplayOrder).ToList();
            parent.TestSections = children;
            if (testSectionQuestionDict.TryGetValue(parent.Id, out var testSectionQuestions))
            {
                parent.TestSectionQuestions = testSectionQuestions;
            }
            if (testAISettingDict.TryGetValue(parent.Id, out var testAISettings))
            {
                parent.TestAISettings = testAISettings;
            }
            foreach (var child in children)
            {
                BuildSectionTreeRecursive(child, allSections, testSectionQuestionDict, testAISettingDict);
            }
        }
    }
}
