// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Shared.Enums;
    using Microsoft.Extensions.DependencyInjection;

    public class TestResultComposite : ResultComposite
    {
        public TestResult TestResult => Result as TestResult;

        public Test? Test { get; set; }

        public override BaseTestStateModel ExportForTestState()
        {
            var childStates = Children.Select(c => c.ExportForTestState()).ToList();

            var stateModel = new TestStateModel
            {
                TestId = TestResult.TestId,
                TestResultId = TestResult.Id,
                PercentResult = TestResult.Percent,
                Status = TestResult.Status,
                Children = childStates,
                UpdatedDate = TestResult?.UpdatedDate ?? TestResult?.CreatedDate
            };
            return stateModel;
        }

        public override async Task Submit(SubmitContext context)
        {
            var skillMatch = Children.FirstOrDefault(x => x.IsBelongTo(context.Id));
            skillMatch?.Submit(context);

            if (Children.All(c => c is TestSectionResultComposite tcr && tcr.TestSectionResult.Status == EnumResultStatus.Done))
            {
                var test = await ServiceProvider.GetRequiredService<ITestService>().GetHierachicalTestById(TestResult.TestId.Value);
                TestResult.Status = EnumResultStatus.Done;
                TestResult.CorrectCount = Children.Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.CorrectCount);
                TestResult.SkillScores = Children.Cast<TestSectionResultComposite>().SelectMany(x =>
                {
                    var correspondSection = test.TestSections.FirstOrDefault(y => y.Id == x.TestSectionResult.TestSectionId);
                    var skillScores = x.TestSectionResult.SkillScores ?? new List<SkillScores>();

                    foreach (var skillScore in skillScores)
                    {
                        skillScore.SkillId = correspondSection?.SkillId;
                        skillScore.SkillName = correspondSection?.Skill?.Name;
                    }

                    return skillScores;
                }).ToList();

                if (context.ScoringFormulaType.HasValue)
                {
                    if (context.ScoringFormulaType == EnumScoringFormulaType.Percent)
                    {
                        TestResult.PercentModule = Children.Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.PercentModule);
                    }
                    else if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore)
                    {
                        // Apply complex scoring formula
                    }
                }
            }
        }

        public override async Task SubmitTest(SubmitContext context)
        {
            var skillMatch = Children.FirstOrDefault(x => x.IsBelongTo(context.Id));
            skillMatch?.SubmitTest(context);
            if (Children.All(c => c is TestSectionResultComposite tcr && tcr.TestSectionResult.Status == EnumResultStatus.Done))
            {
                var test = await ServiceProvider.GetRequiredService<ITestService>().GetHierachicalTestById(TestResult.TestId.Value);
                TestResult.Status = EnumResultStatus.Done;
                TestResult.CorrectCount = Children.Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.CorrectCount);
                TestResult.SkillScores = Children.Cast<TestSectionResultComposite>().SelectMany(x =>
                {
                    var correspondSection = test.TestSections.FirstOrDefault(y => y.Id == x.TestSectionResult.TestSectionId);
                    var skillScores = x.TestSectionResult.SkillScores ?? new List<SkillScores>();

                    foreach (var skillScore in skillScores)
                    {
                        skillScore.SkillId = correspondSection?.SkillId;
                        skillScore.SkillName = correspondSection?.Skill?.Name;
                    }

                    return skillScores;
                }).ToList();

                if (context.ScoringFormulaType.HasValue)
                {
                    if (context.ScoringFormulaType == EnumScoringFormulaType.Percent)
                    {
                        TestResult.PercentModule = Children.Cast<TestSectionResultComposite>().Sum(x => x.TestSectionResult.PercentModule);
                    }
                    else if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore)
                    {
                        // Apply complex scoring formula
                    }
                }
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

            if (Children.Any(x => x is TestSectionResultComposite { TestSectionResult.Status: EnumResultStatus.Process }))
            {
                return;
            }

            var skillsPairCompositeResults = Test?.TestSections.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                var sectionResultComposite =
                    Children.FirstOrDefault(y => y is TestSectionResultComposite tsr && tsr.TestSectionResult.TestSectionId == x.Id) as TestSectionResultComposite;
                return new { Section = x, SectionResultComposite = sectionResultComposite };
            }).Where(x => x.SectionResultComposite != null).ToList();

            var childCanStart =
                skillsPairCompositeResults?.FirstOrDefault(x =>
                    x.SectionResultComposite.TestSectionResult.Status is EnumResultStatus.New or EnumResultStatus.Unfinished)?.SectionResultComposite;

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

        public override async Task LoadTestHierarchicalData()
        {
            if (TestResult.TestId == null)
            {
                return;
            }

            var testService = ServiceProvider.GetRequiredService<ITestService>();
            Test = await testService.GetHierachicalTestById(TestResult.TestId.Value);

            if (Children != null && Test != null)
            {
                foreach (var child in Children.Where(x => x is TestSectionResultComposite).Cast<TestSectionResultComposite>())
                {
                    var testSection = Test.TestSections.FirstOrDefault(x => x.Id == child.TestSectionResult.TestSectionId);
                    if (testSection != null)
                    {
                        child.TestSection = testSection;
                        await child.LoadTestHierarchicalData();
                    }
                }
            }
        }
    }
}
