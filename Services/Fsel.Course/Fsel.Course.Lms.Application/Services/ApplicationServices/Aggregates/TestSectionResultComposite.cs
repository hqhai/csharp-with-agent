// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;

    public class TestSectionResultComposite : ResultComposite
    {
        public TestSectionResult TestSectionResult => (TestSectionResult)Result;

        public override void GenerateChildren()
        {
            if (TestSectionResult.TestAnswers != null && TestSectionResult.TestAnswers.Any())
            {
                foreach (var testAnswer in TestSectionResult.TestAnswers)
                {
                    Children.Add(new TestAnswerLeaf { Result = testAnswer, Parent = this });
                }
            }
            else if (TestSectionResult.SectionResults != null && TestSectionResult.SectionResults.Any())
            {
                foreach (var sectionResult in TestSectionResult.SectionResults)
                {
                    var sectionComposite = new SkillResultComposite { Result = sectionResult, Parent = this };
                    Children.Add(sectionComposite);
                    sectionComposite.GenerateChildren();
                }
            }
        }

        public override void Start()
        {
            if (TestSectionResult.Status is EnumResultStatus.New or EnumResultStatus.Process)
            {
                TestSectionResult.Status = EnumResultStatus.Process;
            }

            if (Children.All(c => c is TestSectionResultComposite))
            {
                foreach (var child in Children)
                {
                    var sectionResultComposite = (TestSectionResultComposite)child;
                    if (sectionResultComposite.TestSectionResult.Status is EnumResultStatus.New or EnumResultStatus.Unfinished)
                    {
                        sectionResultComposite.Start();
                    }
                }
            }
        }

        public override async Task Submit()
        {
            await base.Submit();

            if (Children != null && Children.Count > 0)
            {
                if (Children.All(c => ((TestAnswer)c.Result).Status == Shared.Enums.EnumAnswerStatus.Done))
                {
                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        firstSkillScore.CountQuestion = TestSectionResult.TestAnswers.Count;
                        firstSkillScore.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }
                    TestSectionResult.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                }
                else if (Children.All(c => c is TestSectionResultComposite))
                {
                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        firstSkillScore.CountQuestion = TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CountQuestion);
                        firstSkillScore.CorrectCount = TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CorrectCount);
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }
                    TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(x => x.CorrectCount);
                }
                else
                {
                    // get layout and process
                }

                TestSectionResult.Status = EnumResultStatus.Done;
            }
        }
    }
}
