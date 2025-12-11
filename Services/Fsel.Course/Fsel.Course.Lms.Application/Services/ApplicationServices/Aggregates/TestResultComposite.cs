// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.Extensions.DependencyInjection;

    public class  TestResultComposite : ResultComposite
    {
        public TestResult TestResult => Result as TestResult;

        public override BaseTestStateModel ExportForTestState()
        {
            var childStates = Children?.Select(c => c.ExportState()).ToList() ?? new List<BaseTestStateModel>();

            var stateModel = new TestStateModel
            {
                TestId = TestResult.TestId,
                TestResultId = TestResult.Id,
                PercentResult = TestResult.Percent,
                Status = TestResult.Status,
                Children = childStates,
                StepFlowId = TestResult.StepFlowId,
                UpdatedDate = TestResult?.UpdatedDate ?? TestResult?.CreatedDate
            };
            return stateModel;
        }

        public override async Task Submit()
        {
            await base.Submit();
            var test = await ServiceProvider.GetRequiredService<ITestService>().GetHierachicalTestById(TestResult.TestId.Value);
            if (Children.All(c => c is TestSectionResultComposite tcr && tcr.TestSectionResult.Status == EnumResultStatus.Done))
            {
                TestResult.Status = EnumResultStatus.Done;
                TestResult.CorrectCount = Children.Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.CorrectCount);
                TestResult.SkillScores = Children.Cast<TestSectionResultComposite>().SelectMany(x =>
                {
                    var correspondSection = test.TestSections.FirstOrDefault(y => y.Id == x.TestSectionResult.TestSectionId);
                    var skillScores = x.TestSectionResult.SkillScores ?? new List<SkillScores>();
                    foreach (var skillScore in skillScores)
                    {
                        skillScore.SkillId = correspondSection?.SkillId;
                    }
                    return skillScores;
                }).ToList();
            }
        }

        public override void Start()
        {
            if (TestResult.Status == EnumResultStatus.New)
            {
                TestResult.Status = EnumResultStatus.Process;
            }

            if (Children.Count <= 0)
            {
                return;
            }

            var childCanStart =
                Children.FirstOrDefault(x => x is TestSectionResultComposite
                {
                    TestSectionResult.Status: EnumResultStatus.New or EnumResultStatus.Unfinished
                }) as TestSectionResultComposite;

            childCanStart?.Start();
        }

        public override void GenerateChildren()
        {
            if (TestResult.SectionResults.Any())
            {
                foreach (var sectionResult in TestResult.SectionResults.Where(x => x.ParentTestSectionResultId == null && x.ParentTestSectionResult == null))
                {
                    var sectionComposite = new TestSectionResultComposite { Result = sectionResult, Parent = this, ServiceProvider = ServiceProvider, Name = "Skill" };
                    Children.Add(sectionComposite);
                    sectionComposite.GenerateChildren();
                }
            }
        }

        public override async Task LoadTotalScoreData()
        {
            foreach (var child in Children.Where(x => x is ResultComposite).Cast<ResultComposite>())
            {
                await child.LoadTotalScoreData();
            }

            TestResult.CorrectTotal = Children.Where(x => x is TestSectionResultComposite).Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.CorrectTotal);
        }

        public override BaseTestStateModel ExportState()
        {
            var childStates = Children?.Select(c => c.ExportState()).ToList() ?? new List<BaseTestStateModel>();

            var stateModel = new TestStateModel
            {
                TestId = TestResult.TestId,
                TestResultId = TestResult.Id,
                PercentResult = TestResult.Percent,
                Status = TestResult.Status,
                Children = childStates,
                StepFlowId = TestResult.StepFlowId,
                UpdatedDate = TestResult?.UpdatedDate ?? TestResult?.CreatedDate
            };
            return stateModel;
        }
    }
}
