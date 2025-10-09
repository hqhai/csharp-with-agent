// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface ITestRequestSubmitHandler : IBaseTestRequestHandler
    {
    }

    public class TestRequestSubmitHandler : BaseTestRequestHandler, ITestRequestSubmitHandler
    {
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IFlowService _flowService;
        private readonly ITestService _testService;

        public TestRequestSubmitHandler(IRepository<TestAnswer> testAnswerRepository,
            IRepository<TestGroupResult> testGroupResultRepository,
            IFlowService flowService,
            ITestService testService)
        {
            _testAnswerRepository = testAnswerRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _flowService = flowService;
            _testService = testService;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TestRequestCommand?.IsSubmit == true && context.TestSectionResult != null)
            {
                var testResult = await _testService.LoadHierachicalTestResult(x => x.Id == context.TestSectionResult.TestResultId.Value);

                var skillResult = GetSkillResult(context.TestSectionResult.Id, testResult.SectionResults);

                MarkDoneSectionResult(skillResult);

                testResult.SkillScores = testResult.SectionResults
                         .Where(x => x.Status == EnumResultStatus.Done)
                         .SelectMany(x => x.SkillScores)
                         .ToList();
                testResult.CorrectCount = testResult.SectionResults.Sum(x => x.CorrectCount);

                if (testResult.SectionResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    testResult.Status = EnumResultStatus.Done;

                    var testGroupResult = await _testGroupResultRepository.Queryable.Where(x => x.Id == testResult.TestGroupResultId)
                    .Include(x => x.TestResults)
                    .FirstOrDefaultAsync();

                    if (testGroupResult.TestResults != null && testGroupResult.TestResults.All(x => x.Status == EnumResultStatus.Done))
                    {
                        var nextStepId = await _flowService.GetPTNextModule(testGroupResult.StudentId.Value);

                        if (nextStepId == null)
                        {
                            testGroupResult.Status = EnumResultStatus.Done;
                        }
                        else
                        {
                            context.StartNewModule = true;
                        }
                    }
                }

                await _testAnswerRepository.DbContext.SaveChangesAsync();
            }
        }

        private void MarkDoneSectionResult(TestSectionResult testSectionResult)
        {
            if (testSectionResult.SectionResults.Any())
            {
                foreach (var sectionResult in testSectionResult.SectionResults)
                {
                    MarkDoneSectionResult(sectionResult);
                }

                testSectionResult.Status = EnumResultStatus.Done;

                if (testSectionResult.SkillScores != null && testSectionResult.SkillScores.Any())
                {
                    var firstSkillScore = testSectionResult.SkillScores.First();
                    firstSkillScore.CountQuestion = testSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CountQuestion);
                    firstSkillScore.CorrectCount = testSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CorrectCount);
                    testSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }
                testSectionResult.CorrectCount = testSectionResult.SectionResults.Sum(x => x.CorrectCount);
            }
            else if (testSectionResult.TestAnswers.Any())
            {
                testSectionResult.TestAnswers.ForEach(x => x.Status = EnumAnswerStatus.Done);

                testSectionResult.Status = EnumResultStatus.Done;
                if (testSectionResult.SkillScores != null && testSectionResult.SkillScores.Any())
                {
                    var firstSkillScore = testSectionResult.SkillScores.First();
                    firstSkillScore.CountQuestion = testSectionResult.TestAnswers.Count;
                    firstSkillScore.CorrectCount = testSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                    testSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }
                testSectionResult.CorrectCount = testSectionResult.TestAnswers.Sum(t => t.CorrectCount);
            }
        }

        private TestSectionResult GetSkillResult(Guid childTestSectionResultId, IEnumerable<TestSectionResult> testSectionResults)
        {
            foreach (var sectionResult in testSectionResults)
            {
                if (IsBelongTo(sectionResult, childTestSectionResultId))
                {
                    return sectionResult;
                }
            }

            return null;
        }

        private bool IsBelongTo(TestSectionResult testSectionResult, Guid childTestSectionResultId)
        {
            if (childTestSectionResultId == testSectionResult.Id)
            {
                return true;
            }

            foreach (var sectionResult in testSectionResult.SectionResults)
            {
                if (IsBelongTo(sectionResult, childTestSectionResultId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
