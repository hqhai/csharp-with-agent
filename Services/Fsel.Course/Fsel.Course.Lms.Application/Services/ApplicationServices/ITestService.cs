// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public interface ITestService
    {
        Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate);

        Task<Test> GetHierachicalTestById(Guid id);

        Task<TestResult> LoadHierachicalTestResult(Expression<Func<TestResult, bool>> predicate, bool isReadOnly = false);

        Task<TestGroupResult> InitTestGroupResultForFlow(Guid? flowId, Guid programId, Guid programIdOfPt, Guid studentId, EnumTestType enumTestType, bool isByPass = false);

        Task<TestGroupResult> InitTestGroupResult(Guid studentId, EnumTestType enumTestType, bool isByPass = false);

        Task<TestResult> MakeNewTestResultTree(Guid studentId, Guid stepFlowId, Guid testGroupResultId, Guid programId, Guid? actionFlowId = default);

        Task<TestResult> MakeSectionTestResult(Guid studentId, TestResult testResult, Guid testId);

        Task CreateAnswers(SubmitAnswerCommandModel request);

        Task CreateTestAnswers(SubmitAnswerCommandModel request);
    }

    public class TestService : ITestService
    {
        private readonly ITestCachingService _testCachingService;
        private readonly ITestRepository _testRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IRepository<TestSection> _testSectionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISpeakingEvaluationAIService _evaluationAIService;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly IContinuousPronunciationAssessmentService _continuousPronunciation;

        public TestService(ITestCachingService testCachingService,
            ITestRepository testRepository,
            ICategoryTestBankRepository categoryTestBankRepository,
            IStepFlowRepository stepFlowRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TestGroupResult> testGroupResultRepository,
            IRepository<TestSection> testSectionRepository,
            IQuestionRepository questionRepository,
            ISpeakingEvaluationAIService evaluationAIService,
            IRepository<TestAnswer> testAnswerRepository,
            QuestionConverter questionConverter,
            ITestSectionResultRepository testSectionResultRepository,
            IContinuousPronunciationAssessmentService continuousPronunciation)
        {
            _testCachingService = testCachingService;
            _testRepository = testRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
            _stepFlowRepository = stepFlowRepository;
            _testResultRepository = testResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testSectionRepository = testSectionRepository;
            _questionRepository = questionRepository;
            _evaluationAIService = evaluationAIService;
            _testAnswerRepository = testAnswerRepository;
            _questionConverter = questionConverter;
            _testSectionResultRepository = testSectionResultRepository;
            _continuousPronunciation = continuousPronunciation;
        }

        public async Task<Test> GetHierachicalTestFirstOrDefault(Expression<Func<Test, bool>> predicate)
        {
            var tests = await _testRepository.ReadQueryable
                .Where(predicate)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            var test = tests.FirstOrDefault();
            if (tests.Count > 1)
            {
                var index = new Random().Next(tests.Count);
                test = tests.ElementAtOrDefault(index);
            }

            if (test != null)
            {
                test = await GetHierachicalTestById(test.Id);
            }

            return test;
        }

        public async Task<Test> GetHierachicalTestById(Guid id)
        {
            return await _testCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var test = await _testRepository.ReadQueryable
                    .Where(x => x.Id == id)
                    .Include(x => x.TestSections)
                        .ThenInclude(x => x.Skill)
                    .Include(x => x.TestSections)
                        .ThenInclude(ts => ts.TestSectionQuestions)
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

        public async Task<TestGroupResult> InitTestGroupResult(Guid studentId, EnumTestType enumTestType, bool isByPass = false)
        {
            var testGroupResult = new TestGroupResult { StudentId = studentId, TestType = enumTestType, Status = isByPass ? EnumResultStatus.ByPass : EnumResultStatus.New };
            _testGroupResultRepository.Add(testGroupResult);
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            return testGroupResult;
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
                    var testSectionResult = new TestSectionResult { TestSectionId = section.Id, StudentId = studentId, Status = EnumResultStatus.New };
                    testResult.SectionResults.Add(testSectionResult);

                    CreateTestSectionResultTree(section, testSectionResult, testResult, section.LayoutType);
                }

                return testResult;
            }

            return null;
        }

        public async Task<TestResult> MakeSectionTestResult(Guid studentId, TestResult testResult, Guid testId)
        {
            ArgumentNullException.ThrowIfNull(testResult);
            var test = await GetHierachicalTestFirstOrDefault(x => x.Id == testId);
            if (test != null)
            {
                foreach (var section in test.TestSections)
                {
                    var testSectionResult = new TestSectionResult
                    {
                        TestSectionId = section.Id,
                        StudentId = studentId,
                        Status = EnumResultStatus.New,
                        TestResultId = testResult.Id
                    };
                    testResult.SectionResults.Add(testSectionResult);
                    _testSectionResultRepository.Add(testSectionResult);
                    CreateTestSectionResultTree(section, testSectionResult, testResult, section.LayoutType);
                }

                await _testSectionResultRepository.UnitOfWork.SaveChangesAsync();

                return testResult;
            }

            return null;
        }

        public async Task<TestGroupResult> InitTestGroupResultForFlow(Guid? flowId, Guid programId, Guid programIdOfPt, Guid studentId, EnumTestType enumTestType, bool isByPass = false)
        {
            var testGroupResult = new TestGroupResult
            {
                ProgramId = programId,
                ProgramIdOfPt = programIdOfPt,
                FlowId = flowId,
                StudentId = studentId,
                TestType = enumTestType,
                Status = isByPass ? EnumResultStatus.ByPass : EnumResultStatus.New
            };
            _testGroupResultRepository.Add(testGroupResult);
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            return testGroupResult;
        }

        private static void CreateTestSectionResultTree(TestSection parentTestSection, TestSectionResult parentSectionResult, TestResult testResult, EnumTestLayoutType? layoutType = null)
        {
            foreach (var child in parentTestSection.TestSections)
            {
                if (IsInvalidSection(child, layoutType))
                {
                    continue;
                }

                var testSectionResult = new TestSectionResult
                {
                    TestSectionId = child.Id,
                    StudentId = parentSectionResult.StudentId,
                    Status = EnumResultStatus.New,
                    TestResultId = testResult.Id,
                    ParentTestSectionResult = parentSectionResult
                };
                parentSectionResult.SectionResults.Add(testSectionResult);
                testResult.SectionResults.Add(testSectionResult);

                if (layoutType == EnumTestLayoutType.Basic)
                {
                    CreateTestSectionResultTree(child, testSectionResult, testResult, layoutType);
                }
            }
        }

        private static bool IsDeepestSection(TestSection testSection)
        {
            return testSection.TestSections == null || !testSection.TestSections.Any();
        }

        /// <summary>
        /// Assume only Basic layout type section can have questions and loaded questions completely
        /// </summary>
        /// <param name="testSection"></param>
        /// <param name="layoutType"></param>
        /// <returns></returns>
        private static bool IsInvalidSection(TestSection testSection, EnumTestLayoutType? layoutType)
        {
            if (IsDeepestSection(testSection) && layoutType == EnumTestLayoutType.Basic)
            {
                if (testSection.TestSectionQuestions == null || !testSection.TestSectionQuestions.Any())
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<TestResult> LoadHierachicalTestResult(Expression<Func<TestResult, bool>> predicate, bool isReadOnly = false)
        {
            var queryable = isReadOnly ? _testResultRepository.ReadQueryable : _testResultRepository.Queryable;

            return await queryable
                .Where(predicate)
                .Include(x => x.SectionResults)
                .ThenInclude(x => x.TestSection)
                .ThenInclude(x => x.TestSectionQuestions)
                .Include(x => x.TestAnswers)
                .FirstOrDefaultAsync() ?? new TestResult();
        }

        public async Task CreateAnswers(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.Answers == null || !request.Answers.Any())
            {
                return;
            }

            var testSectionResult = await _testSectionResultRepository.Queryable.Include(x => x.TestSection)
                                                                      .Include(x => x.SectionResults)
                                                                      .Where(x => x.Id == request.SectionResultId)
                                                                      .FirstOrDefaultAsync();
            if (testSectionResult == null || testSectionResult.Status == EnumResultStatus.Done)
            {
                throw new InvalidOperationException("TestSectionResult Invalid");
            }
            await SubmitQuestions(request);
        }

        public async Task CreateTestAnswers(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.Answers == null || !request.Answers.Any())
            {
                return;
            }

            var testSectionResult = await _testSectionResultRepository.Queryable.Include(x => x.TestSection)
                                                                .Where(x => x.Id == request.SectionResultId)
                                                                .FirstOrDefaultAsync();
            if (testSectionResult == null)
            {
                return;
            }
            var layoutType = testSectionResult.TestSection?.LayoutType;
            if (layoutType == EnumTestLayoutType.Basic)
            {
                await SubmitTestQuestions(request);
            }
            else if (layoutType == EnumTestLayoutType.SpeakingMocktest)
            {
                await SubmitTimeCodes(request, testSectionResult);
            }
            else if (layoutType == EnumTestLayoutType.WritingMocktest)
            {
                await SubmitSections(request);
            }
        }

        private async Task UpdateTestSecionResult(TestSectionResult testSectionResult, IList<TestSection> testSections)
        {
            var currentTimeCodeId = testSectionResult.CurrentSectionTimeCodeId;

            if (testSectionResult.CurrentSectionTimeCodeId.HasValue)
            {
                var currentSectionTimeCode = await _testSectionRepository.GetByIdAsync(testSectionResult.CurrentSectionTimeCodeId.Value);
                if (currentSectionTimeCode == null)
                {
                    return;
                }
                if (testSections.Any(x => x.Config?.DisplayTime > currentSectionTimeCode.Config?.DisplayTime))
                {
                    testSectionResult.CurrentSectionTimeCodeId = testSections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
                }
            }
            else
            {
                testSectionResult.CurrentSectionTimeCodeId = testSections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
            }

            if (currentTimeCodeId != testSectionResult.CurrentSectionTimeCodeId)
            {
                await _testSectionResultRepository.BulkUpdateList(new List<TestSectionResult> { testSectionResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.CurrentSectionTimeCodeId };
                });
            }
        }

        private async Task SubmitSections(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var sectionTimeCodeIds = request.Answers.Where(x => x.TestSectionId.HasValue).Select(x => x.TestSectionId!.Value).ToList();
            var testSections = await _testSectionRepository.GetByIdsAsync(sectionTimeCodeIds);
            if (testSections.Any())
            {
                var sectionIds = testSections.Select(x => x.Id).ToList();

                var partResults = await _testSectionResultRepository.ReadQueryable.Where(x => x.TestSectionId.HasValue && sectionIds.Contains(x.TestSectionId.Value))
                                                                    .Where(x => x.TestResultId == request.TestResultId)
                                                                    .ToListAsync();
                var testSectionResultIds = partResults.Select(x => x.Id).ToList();
                var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId.HasValue && testSectionResultIds.Contains(x.TestSectionResultId.Value)).ToListAsync();

                foreach (var item in request.Answers)
                {
                    var testSection = testSections.FirstOrDefault(x => x.Id == item.TestSectionId);
                    if (testSection == null)
                    {
                        return;
                    }
                    var partId = testSection.Id;
                    var partResult = partResults.FirstOrDefault(x => x.TestSectionId == partId);
                    if (partResult == null)
                    {
                        return;
                    }

                    int answerLength = item.Answer?.ToString()?.Length ?? default;
                    if (testSection.DisplayOrder == AnswerLength.Section0 && answerLength > AnswerLength.MaxLengthDisplayOrder0)
                    {
                        return;
                    }
                    if (testSection.DisplayOrder == AnswerLength.Section1 && answerLength > AnswerLength.MaxLengthDisplayOrder1)
                    {
                        return;
                    }

                    var testAnswer = testAnswers.FirstOrDefault(x => x.TestSectionId == testSection.Id);
                    if (testAnswer == null)
                    {
                        testAnswer = new TestAnswer
                        {
                            TestResultId = request.TestResultId,
                            TestSectionResultId = partResult.Id,
                            TestSectionId = item.TestSectionId,
                            StudentId = request.StudentId ?? partResult.StudentId
                        };
                        _testAnswerRepository.Add(testAnswer);
                    }
                    testAnswer.Answer = item.Answer;
                    testAnswer.Status = EnumAnswerStatus.Done;
                    if (!testAnswer.IsValid())
                    {
                        return;
                    }
                }

                await _testAnswerRepository.DbContext.SaveChangesAsync();
            }
        }

        private async Task SubmitTimeCodes(SubmitAnswerCommandModel request, TestSectionResult testSectionResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(testSectionResult);
            var sectionTimeCodeIds = request.Answers.Where(x => x.TestSectionId.HasValue).Select(x => x.TestSectionId!.Value).ToList();
            var testSections = await _testSectionRepository.GetByIdsAsync(sectionTimeCodeIds);

            if (testSections.Any())
            {
                await UpdateTestSecionResult(testSectionResult, testSections.ToList());
                var sectionIds = testSections.Select(x => x.ParentId).ToList();
                var partResults = await _testSectionResultRepository.ReadQueryable.Where(x => x.TestSectionId.HasValue && sectionIds.Contains(x.TestSectionId.Value))
                                                                    .Where(x => x.TestResultId == request.TestResultId)
                                                                    .ToListAsync();

                var testSectionResultIds = partResults.Select(x => x.Id).ToList();
                var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId.HasValue && testSectionResultIds.Contains(x.TestSectionResultId.Value)).ToListAsync();

                foreach (var item in request.Answers)
                {
                    var testSection = testSections.FirstOrDefault(x => x.Id == item.TestSectionId);
                    if (testSection == null)
                    {
                        return;
                    }
                    var partId = testSection.ParentId;
                    var partResult = partResults.FirstOrDefault(x => x.TestSectionId == partId);
                    if (partResult == null)
                    {
                        return;
                    }

                    var pronunciation = await _continuousPronunciation.AssessPronunciationFromFileContinuousAsync(item.Answer?.ToString() ?? string.Empty, item.SpeechTextAnswer ?? string.Empty);
                    var testAnswer = testAnswers.FirstOrDefault(x => x.TestSectionId == testSection.Id);
                    if (testAnswer == null)
                    {
                        testAnswer = new TestAnswer
                        {
                            TestResultId = request.TestResultId,
                            TestSectionResultId = partResult.Id,
                            TestSectionId = item.TestSectionId,
                            StudentId = request.StudentId ?? partResult.StudentId
                        };
                        _testAnswerRepository.Add(testAnswer);
                    }
                    testAnswer.SpeechTextAnswer = item.SpeechTextAnswer;
                    testAnswer.PronunciationScore = pronunciation.PronunciationScore;
                    testAnswer.Answer = item.Answer;
                    testAnswer.Status = EnumAnswerStatus.Done;
                    if (!testAnswer.IsValid())
                    {
                        return;
                    }
                }

                await _testAnswerRepository.DbContext.SaveChangesAsync();
            }
        }

        private async Task SubmitTestQuestions(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
            var questions = await _questionRepository.ReadQueryable.Include(x => x.TestSectionQuestions)
                                                     .Where(x => questionIds.Contains(x.Id))
                                                     .ToListAsync();
            if (questions != null && questions.Any())
            {
                var partIds = questions.SelectMany(x => x.TestSectionQuestions).Select(x => x.TestSectionId).ToList();

                var partResults = await _testSectionResultRepository.ReadQueryable.Where(x => x.TestSectionId.HasValue && partIds.Contains(x.TestSectionId.Value))
                                                                           .Where(x => x.TestResultId == request.TestResultId)
                                                                           .ToListAsync();
                var partResultIds = partResults.Select(x => x.Id).ToList();
                var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId.HasValue && partResultIds.Contains(x.TestSectionResultId.Value))
                                                             .ToListAsync();
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    if (question == null)
                    {
                        return;
                    }
                    var partId = question.TestSectionQuestions.First().TestSectionId;
                    var partResult = partResults.FirstOrDefault(x => x.TestSectionId == partId);
                    if (partResult == null)
                    {
                        return;
                    }

                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        return;
                    }

                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var testAnswer = testAnswers.FirstOrDefault(x => x.QuestionId == question.Id);
                    if (testAnswer == null)
                    {
                        testAnswer = new TestAnswer
                        {
                            TestSectionResultId = partResult.Id,
                            TestResultId = request.TestResultId,
                            QuestionId = question.Id,
                            TestSectionId = partResult.TestSectionId,
                            StudentId = request.StudentId ?? partResult.StudentId
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

        private async Task SubmitQuestions(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
            var questions = await _questionRepository.ReadQueryable.Include(x => x.TestSectionQuestions)
                                                     .Where(x => questionIds.Contains(x.Id))
                                                     .ToListAsync();
            if (questions != null && questions.Any())
            {
                var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId == request.SectionResultId).ToListAsync();
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    if (question == null)
                    {
                        return;
                    }
                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        return;
                    }

                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var testAnswer = testAnswers.FirstOrDefault(x => x.QuestionId == question.Id);
                    if (testAnswer == null)
                    {
                        testAnswer = new TestAnswer
                        {
                            TestResultId = request.TestResultId,
                            TestSectionResultId = request.SectionResultId,
                            QuestionId = question.Id,
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
