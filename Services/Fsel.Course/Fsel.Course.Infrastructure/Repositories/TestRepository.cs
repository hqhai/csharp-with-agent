// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Common.Enums;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class TestRepository : BaseRepository<Test>, ITestRepository
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public TestRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            ITestResultRepository testResultRepository,
            ITestGroupResultRepository testGroupResultRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _testResultRepository = testResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
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

        public async Task<(IDictionary<Guid, (Test, TestGroupResult, TestResult)>, IDictionary<Guid, Test>)> BuildTestLookupsAsync(UnitResult unitResult, IList<UnitModule> unitModules)
        {
            var testOriginalIds = unitModules
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Test)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!testOriginalIds.Any())
            {
                return (new Dictionary<Guid, (Test, TestGroupResult, TestResult)>(),
                        new Dictionary<Guid, Test>());
            }

            var testResults = await (from baseQ in _testGroupResultRepository.ReadQueryable
                                     where baseQ.UnitResultId == unitResult.Id
                                     join result in _testResultRepository.ReadQueryable on baseQ.Id equals result.TestGroupResultId
                                     select new
                                     {
                                         Test = result.Test,
                                         TestGroupResult = baseQ,
                                         TestResult = result
                                     })
                                     .ToListAsync();

            var testOriginalIdsHasResult = testResults
                .Where(x => x.Test != null)
                .Select(x => x.Test!.OriginalId)
                .Distinct();

            var pendingTestOriginalIds = testOriginalIds
                .Except(testOriginalIdsHasResult)
                .ToList();

            var testDics = await GetTestDicAsync(pendingTestOriginalIds);

            var testResultsByOriginalId = testResults
                .Where(x => x.Test != null)
                .ToDictionary(
                    x => x.Test!.OriginalId,
                    x => (Test: x.Test, TestGroupResult: x.TestGroupResult, TestResult: x.TestResult));

            return (testResultsByOriginalId, testDics);
        }

        public async Task<IDictionary<Guid, Test>> GetTestDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, Test>();
            }

            var tests = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .ToListAsync();

            return tests.ToDictionary(x => x.OriginalId);
        }

        public async Task<(IDictionary<Guid, (Test, TestGroupResult, TestResult)>, IDictionary<Guid, Test>)> BuildTestLookupsAsync(CourseResult courseResult, IList<CourseModule> courseModules)
        {
            var testOriginalIds = courseModules
                .Where(x => x.CourseConfigType == EnumCourseConfigType.Test)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!testOriginalIds.Any())
            {
                return (new Dictionary<Guid, (Test, TestGroupResult, TestResult)>(),
                        new Dictionary<Guid, Test>());
            }

            var testResults = await (from baseQ in _testGroupResultRepository.ReadQueryable
                                     where baseQ.CourseResultId == courseResult.Id
                                     join result in _testResultRepository.ReadQueryable
                                         on baseQ.Id equals result.TestGroupResultId
                                     select new
                                     {
                                         Test = result.Test,
                                         TestGroupResult = baseQ,
                                         TestResult = result
                                     })
                                     .ToListAsync();

            var testOriginalIdsHasResult = testResults
                .Where(x => x.Test != null)
                .Select(x => x.Test!.OriginalId)
                .Distinct();

            var pendingTestOriginalIds = testOriginalIds
                .Except(testOriginalIdsHasResult)
                .ToList();

            var testDics = await GetTestDicAsync(pendingTestOriginalIds);

            var testResultsByOriginalId = testResults
                .Where(x => x.Test != null)
                .ToDictionary(
                    x => x.Test!.OriginalId,
                    x => (Test: x.Test, TestGroupResult: x.TestGroupResult, TestResult: x.TestResult));

            return (testResultsByOriginalId, testDics);
        }
    }
}
