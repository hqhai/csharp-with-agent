// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ITestService
    {
        Task<TestResult> InitTestForStepFlow(Guid studentId, Guid stepFlowId, Guid testGroupResultId, Guid programId, Guid? actionFlowId = default);

        Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate);

        Task<TestGroupResult> InitTestGroupResultForFlow(Guid flowId, Guid studentId, EnumTestType enumTestType);

        Task<TestSectionModel> GetQuestionsOfTestSection(Guid testSectionResultId);
    }

    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IRepository<TestSectionResult> _testSectionResultRepository;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IMapper _mapper;

        public TestService(ITestRepository testRepository,
             ICategoryTestBankRepository categoryTestBankRepository,
             IStepFlowRepository stepFlowRepository,
             ITestSectionRepository testSectionRepository,
             IRepository<TestResult> testResultRepository,
             IRepository<TestGroupResult> testGroupResultRepository,
             IRepository<TestSectionResult> testSectionResultRepository,
             ITestSectionQuestionRepository testSectionQuestionRepository,
             IMapper mapper)
        {
            _testRepository = testRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
            _stepFlowRepository = stepFlowRepository;
            _testSectionRepository = testSectionRepository;
            _testResultRepository = testResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testSectionResultRepository = testSectionResultRepository;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _mapper = mapper;
        }

        public async Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate)
        {
            var test = await _testRepository.ReadQueryable
                                 .Where(predicate)
                                 .Include(x => x.TestSections)
                                 .OrderBy(x => x.CreatedDate)
                                 .FirstOrDefaultAsync();

            foreach (var section in test.TestSections)
            {
                await LoadTestSectionTree(section);
            }

            return test;
        }

        public async Task<TestResult> InitTestForStepFlow(Guid studentId,
            Guid stepFlowId,
            Guid testGroupResultId,
            Guid programId,
            Guid? actionFlowId = default)
        {
            var stepFlow = await _stepFlowRepository.ReadQueryable
                                .Where(x => x.Id == stepFlowId)
                                .FirstOrDefaultAsync();
            var testIds = await _categoryTestBankRepository.ReadQueryable
                                        .Where(x => x.ProgramId == programId && x.TestType == EnumTestType.PlacementTest)
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(x => x.TestOriginalId)
                                        .ToListAsync();

            var test = await GetHierachicalTestFirstOrDefault(x => x.LevelId == stepFlow.LevelId
                                    && testIds.Contains(x.OriginalId)
                                    && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion);
            if (test != null)
            {
                var testResult = new TestResult
                {
                    TestId = test.Id,
                    StudentId = studentId,
                    Status = EnumResultStatus.New,
                    StepFlowId = stepFlowId,
                    ActionFlowId = actionFlowId,
                    TestGroupResultId = testGroupResultId,
                };

                foreach (var section in test.TestSections)
                {
                    var testSectionResult = new TestSectionResult
                    {
                        TestSectionId = section.Id,
                        TestResultId = testResult.Id,
                        StudentId = studentId,
                        Status = EnumResultStatus.New,
                    };
                    testResult.SectionResults.Add(testSectionResult);

                    CreateTestSectionResultTree(section, testSectionResult);
                }

                await FillTotalScoreForTestResult(testResult);

                _testResultRepository.Add(testResult);
                testResult.Status = EnumResultStatus.Process;
                var firstSkill = testResult.SectionResults.First();
                firstSkill.Status = EnumResultStatus.Process;

                await _testResultRepository.UnitOfWork.SaveChangesAsync();

                return testResult;
            }

            return null;
        }

        public async Task FillTotalScoreForTestResult(TestResult testResult)
        {
            foreach (var childSectionResult in testResult.SectionResults)
            {
                await FillTotalScoreForSectionResult(childSectionResult);
            }

            testResult.CorrectTotal = testResult.SectionResults.Sum(x => x.CorrectTotal);
        }

        public async Task FillTotalScoreForSectionResult(TestSectionResult testSectionResult)
        {
            if (testSectionResult.SectionResults.Count == 0)
            {
                var questions = await _testSectionQuestionRepository.ReadQueryable.Where(x => x.TestSectionId == testSectionResult.TestSectionId)
                    .Include(x => x.Question)
                    .Select(x => x.Question)
                    .ToListAsync();

                testSectionResult.CorrectTotal = questions.Sum(x => x.CorrectTotal);
                testSectionResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        TotalCount = questions.Sum(x => x.CorrectTotal),
                        TotalQuestion = questions.Count
                    }
                };
            }
            else
            {
                foreach (var childSectionResult in testSectionResult.SectionResults)
                {
                    await FillTotalScoreForSectionResult(childSectionResult);
                }

                testSectionResult.CorrectTotal = testSectionResult.SectionResults.Sum(x => x.CorrectTotal);
                testSectionResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        TotalCount =  testSectionResult.SectionResults.Sum(x => x.CorrectTotal),
                        TotalQuestion =  testSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.TotalQuestion)
                    }
                };
            }
        }

        public async Task<TestGroupResult> InitTestGroupResultForFlow(Guid flowId, Guid studentId, EnumTestType enumTestType)
        {
            var testGroupResult = new TestGroupResult
            {
                FlowId = flowId,
                StudentId = studentId,
                TestType = enumTestType,
                Status = EnumResultStatus.New
            };
            _testGroupResultRepository.Add(testGroupResult);
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            return testGroupResult;
        }

        public async Task<TestSectionModel> GetQuestionsOfTestSection(Guid testSectionResultId)
        {
            var testSectionResult = await _testSectionResultRepository.ReadQueryable
                                        .Where(x => x.Id == testSectionResultId)
                                        .Include(x => x.SectionResults)
                                        .Include(x => x.TestAnswers)
                                        .FirstOrDefaultAsync();

            if (testSectionResult?.SectionResults != null)
            {
                foreach (var sectionResult in testSectionResult.SectionResults)
                {
                    await LoadTestSectionResultTree(sectionResult);
                }
            }

            var section = await _testSectionRepository.ReadQueryable.Where(x => x.Id == testSectionResult.TestSectionId)
                .Include(x => x.TestSections)
                .FirstOrDefaultAsync();

            if (section.TestSections != null)
            {
                foreach (var child in section.TestSections)
                {
                    await LoadTestSectionTree(child);
                }
            }

            await LoadQuestionForSection(section);

            var testSection = new TestSectionModel();
            _mapper.Map(section, testSection);
            return testSection;
        }

        private static void CreateTestSectionResultTree(TestSection parentTestSection, TestSectionResult parentSectionResult)
        {
            foreach (var child in parentTestSection.TestSections)
            {
                var testSectionResult = new TestSectionResult
                {
                    TestSectionId = child.Id,
                    TestResultId = parentSectionResult.TestResultId,
                    StudentId = parentSectionResult.StudentId,
                    Status = EnumResultStatus.New,
                    ParentTestSectionResultId = parentSectionResult.Id
                };
                parentSectionResult.SectionResults.Add(testSectionResult);
                CreateTestSectionResultTree(child, testSectionResult);
            }
        }

        private async Task LoadTestSectionTree(TestSection testSection)
        {
            testSection.TestSections = await _testSectionRepository.ReadQueryable
                                                .Where(x => x.ParentId == testSection.Id)
                                                .ToListAsync();
            foreach (var child in testSection.TestSections)
            {
                await LoadTestSectionTree(child);
            }
        }

        private async Task LoadQuestionForSection(TestSection testSection)
        {
            if (testSection.TestSections == null || !testSection.TestSections.Any())
            {
                var questions = await _testSectionQuestionRepository.ReadQueryable.Where(x => x.TestSectionId == testSection.Id)
                    .Include(x => x.Question)
                    .ToListAsync();
                testSection.TestSectionQuestions = questions;
            }
            else
            {
                foreach (var child in testSection.TestSections)
                {
                    await LoadQuestionForSection(child);
                }
            }
        }

        private async Task LoadTestSectionResultTree(TestSectionResult testSectionResult)
        {
            testSectionResult.SectionResults = await _testSectionResultRepository.ReadQueryable
                                                .Where(x => x.ParentTestSectionResultId == testSectionResult.Id)
                                                .ToListAsync();

            if (testSectionResult?.SectionResults != null)
            {
                foreach (var sectionResult in testSectionResult.SectionResults)
                {
                    await LoadTestSectionResultTree(sectionResult);
                }
            }
            else
            {
                testSectionResult.TestAnswers = await _testSectionResultRepository.DbContext.Set<TestAnswer>()
                                                    .Where(x => x.TestSectionResultId == testSectionResult.Id)
                                                    .AsNoTracking()
                                                    .ToListAsync();
            }
        }
    }
}
