// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface ITestService
    {
        Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate);

        Task<TestResult> LoadHierachicalTestResult(Expression<Func<TestResult, bool>> predicate, bool isReadOnly = false);

        Task<TestGroupResult> InitTestGroupResultForFlow(Guid flowId, Guid programId, Guid studentId, EnumTestType enumTestType);

        Task<TestResult> MakeNewTestResultTree(Guid studentId, Guid stepFlowId, Guid testGroupResultId, Guid programId, Guid? actionFlowId = default);

        Task CreateAnswers(SubmitAnswerCommandModel request);
    }

    public class TestService : ITestService
    {
        private readonly ITestCachingService _testCachingService;
        private readonly ITestRepository _testRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly Core.Base.Interfaces.IRepository<TestResult> _testResultRepository;
        private readonly Core.Base.Interfaces.IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly Core.Base.Interfaces.IRepository<TestAnswer> _testAnswerRepository;
        private readonly QuestionConverter _questionConverter;

        public TestService(ITestCachingService testCachingService,
            ITestRepository testRepository,
             ICategoryTestBankRepository categoryTestBankRepository,
             IStepFlowRepository stepFlowRepository,
             Core.Base.Interfaces.IRepository<TestResult> testResultRepository,
             Core.Base.Interfaces.IRepository<TestGroupResult> testGroupResultRepository,
             IQuestionRepository questionRepository,
             Core.Base.Interfaces.IRepository<TestAnswer> testAnswerRepository,
             QuestionConverter questionConverter)
        {
            _testCachingService = testCachingService;
            _testRepository = testRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
            _stepFlowRepository = stepFlowRepository;
            _testResultRepository = testResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _questionRepository = questionRepository;
            _testAnswerRepository = testAnswerRepository;
            _questionConverter = questionConverter;
        }

        public async Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate)
        {
            var test = await _testRepository.ReadQueryable
                                 .Where(predicate)
                                 .OrderBy(x => x.CreatedDate)
                                 .FirstOrDefaultAsync();

            if (test != null)
            {
                test = await GetHierachicalTestById(test.Id);
            }

            return test;
        }

        private async Task<Test> GetHierachicalTestById(Guid id)
        {
            return await _testCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var test = await _testRepository.ReadQueryable
                                 .Where(x => x.Id == id)
                                 .Include(x => x.TestSections)
                                 .OrderBy(x => x.CreatedDate)
                                 .FirstOrDefaultAsync(cancellationToken: _);

                var childSections = test.TestSections.Where(x => x.ParentId != null).ToList();
                test.TestSections = test.TestSections.Where(x => x.ParentId == null).ToList();

                foreach (var section in test.TestSections)
                {
                    LoadTestSectionTree(section, childSections);
                }

                return test;
            });
        }

        private void LoadTestSectionTree(TestSection testSection, List<TestSection> inventory)
        {
            testSection.TestSections = inventory.Where(x => x.ParentId == testSection.Id).ToList();

            inventory = inventory.Except(testSection.TestSections).ToList();
            foreach (var child in testSection.TestSections)
            {
                LoadTestSectionTree(child, inventory);
            }
        }

        public async Task<TestResult> MakeNewTestResultTree(Guid studentId,
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
                        StudentId = studentId,
                        Status = EnumResultStatus.New,
                    };
                    testResult.SectionResults.Add(testSectionResult);

                    CreateTestSectionResultTree(section, testSectionResult, testResult);
                }

                return testResult;
            }

            return null;
        }

        public async Task<TestGroupResult> InitTestGroupResultForFlow(Guid flowId, Guid programId, Guid studentId, EnumTestType enumTestType)
        {
            var testGroupResult = new TestGroupResult
            {
                ProgramId = programId,
                FlowId = flowId,
                StudentId = studentId,
                TestType = enumTestType,
                Status = EnumResultStatus.New
            };
            _testGroupResultRepository.Add(testGroupResult);
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            return testGroupResult;
        }

        private static void CreateTestSectionResultTree(TestSection parentTestSection, TestSectionResult parentSectionResult, TestResult testResult)
        {
            foreach (var child in parentTestSection.TestSections)
            {
                var testSectionResult = new TestSectionResult
                {
                    TestSectionId = child.Id,
                    StudentId = parentSectionResult.StudentId,
                    Status = EnumResultStatus.New,
                };
                parentSectionResult.SectionResults.Add(testSectionResult);
                testResult.SectionResults.Add(testSectionResult);
                CreateTestSectionResultTree(child, testSectionResult, testResult);
            }
        }

        public async Task<TestResult> LoadHierachicalTestResult(Expression<Func<TestResult, bool>> predicate, bool isReadOnly = false)
        {
            var queryable = isReadOnly ? _testResultRepository.ReadQueryable : _testResultRepository.Queryable;

            var testResult = await queryable
                                .Where(predicate)
                                .Include(x => x.SectionResults)
                                .ThenInclude(x => x.TestAnswers)
                                .FirstOrDefaultAsync();
            if (testResult == null)
            {
                return null;
            }
            var inventory = testResult.SectionResults.Where(x => x.ParentTestSectionResultId != null).ToList();
            testResult.SectionResults = testResult.SectionResults.Where(x => x.ParentTestSectionResultId == null).ToList();
            foreach (var sectionResult in testResult.SectionResults)
            {
                LoadTestSectionResultTreeRecursive(sectionResult, inventory);
            }
            return testResult;
        }

        private static void LoadTestSectionResultTreeRecursive(TestSectionResult testSectionResult, List<TestSectionResult> inventory)
        {
            testSectionResult.SectionResults = inventory.Where(x => x.ParentTestSectionResultId == testSectionResult.Id).ToList();
            inventory = inventory.Except(testSectionResult.SectionResults).ToList();
            foreach (var child in testSectionResult.SectionResults)
            {
                LoadTestSectionResultTreeRecursive(child, inventory);
            }
        }

        public async Task CreateAnswers(SubmitAnswerCommandModel request)
        {
            if (request.Answers == null || !request.Answers.Any())
            {
                return;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions != null && questions.Any())
            {
                var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId == request.SectionResultId).ToListAsync();
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        return;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var questionId = questionItem.TestSectionQuestions.FirstOrDefault()?.QuestionId ?? default;
                    var testAnswer = testAnswers.FirstOrDefault(x => x.QuestionId == questionId);
                    if (testAnswer == null)
                    {
                        testAnswer = new TestAnswer
                        {
                            TestSectionResultId = request.SectionResultId,
                            QuestionId = questionId,
                            StudentId = request.StudentId
                        };

                        _testAnswerRepository.Add(testAnswer);
                    }

                    testAnswer.Answer = answerConfig;
                    testAnswer.CorrectCount = correctCount;
                    testAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
                    testAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;

                    if (!testAnswer.IsValid())
                    {
                        return;
                    }
                }

                await _testAnswerRepository.DbContext.SaveChangesAsync();
            }
        }
    }
}
