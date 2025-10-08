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
        private readonly IRepository<TestSectionResult> _testSectionResultRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IFlowService _flowService;

        public TestRequestSubmitHandler(IRepository<TestAnswer> testAnswerRepository,
            IRepository<TestSectionResult> testSectionResultRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TestGroupResult> testGroupResultRepository,
            IFlowService flowService)
        {
            _testAnswerRepository = testAnswerRepository;
            _testSectionResultRepository = testSectionResultRepository;
            _testResultRepository = testResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _flowService = flowService;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TestRequestCommand?.IsSubmit == true && context.TestSectionResult != null)
            {
                var skillResultId = context.TestSectionResult.ParentTestSectionResultId;
                var skillResult = await _testSectionResultRepository.Queryable.Where(x => x.Id == skillResultId)
                    .Include(x => x.SectionResults)
                    .ThenInclude(x => x.TestAnswers)
                    .FirstOrDefaultAsync();

                #region update excercise results

                var excercisesResult = skillResult.SectionResults;
                excercisesResult.SelectMany(x => x.TestAnswers).ForEach(x => x.Status = EnumAnswerStatus.Done);
                excercisesResult.ForEach(x =>
                {
                    x.Status = EnumResultStatus.Done;
                    if (x.SkillScores != null && x.SkillScores.Any())
                    {
                        var firstSkillScore = x.SkillScores.First();
                        firstSkillScore.CountQuestion = x.TestAnswers.Count;
                        firstSkillScore.CorrectCount = x.TestAnswers.Sum(t => t.CorrectCount);
                        x.SkillScores = new List<SkillScores> { firstSkillScore };
                    }
                    x.CorrectCount = x.TestAnswers.Sum(t => t.CorrectCount);
                });

                #endregion update excercise results

                #region update skill result

                skillResult.Status = EnumResultStatus.Done;

                if (skillResult.SkillScores != null && skillResult.SkillScores.Any())
                {
                    var firstSkillScore = skillResult.SkillScores.First();
                    firstSkillScore.CountQuestion = skillResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CountQuestion);
                    firstSkillScore.CorrectCount = skillResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CorrectCount);
                    skillResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }
                skillResult.CorrectCount = skillResult.SectionResults.Sum(x => x.CorrectCount);

                #endregion update skill result

                await _testAnswerRepository.DbContext.SaveChangesAsync();

                #region update test result

                var testResult = await _testResultRepository.Queryable.Where(x => x.Id == skillResult.TestResultId)
                    .Include(x => x.SectionResults)
                    .FirstOrDefaultAsync();

                testResult.SkillScores = testResult.SectionResults
                         .Where(x => x.Status == EnumResultStatus.Done)
                         .SelectMany(x => x.SkillScores)
                         .ToList();
                testResult.CorrectCount = testResult.SectionResults.Sum(x => x.CorrectCount);
                #endregion update test result

                if (testResult.SectionResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    testResult.Status = EnumResultStatus.Done;

                    var testGroupResult = await _testGroupResultRepository.Queryable.Where(x => x.Id == testResult.TestGroupResultId)
                    .Include(x => x.TestResults)
                    .FirstOrDefaultAsync();

                    if (testGroupResult.TestResults != null && testGroupResult.TestResults.All(x => x.Status == EnumResultStatus.Done))
                    {
                        var allBranchOfFlow = await _flowService.GetAllFlowBranches(testGroupResult.FlowId.Value);

                        var numberOfModule = allBranchOfFlow.First().Count;

                        if (testGroupResult.TestResults.Count == numberOfModule)
                        {
                            testResult.Status = EnumResultStatus.Done;
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
    }
}
