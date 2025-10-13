// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class TestSectionResultComposite : ResultComposite
    {
        public TestSectionResult TestSectionResult => (TestSectionResult)Result;

        public override BaseTestStateModel ExportState()
        {
            var sectionState = new SectionStateModel
            {
                SectionId = TestSectionResult.TestSectionId,
                SectionResultId = TestSectionResult.Id,
                Status = TestSectionResult.Status,
                Children = new List<BaseTestStateModel>()
            };
            if (Children != null && Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    sectionState.Children.Add(child.ExportState());
                }
            }
            return sectionState;
        }

        public override void GenerateChildren()
        {
            if (TestSectionResult.TestAnswers != null && TestSectionResult.TestAnswers.Any())
            {
                foreach (var testAnswer in TestSectionResult.TestAnswers)
                {
                    Children.Add(new TestAnswerLeaf { Result = testAnswer, Parent = this, ServiceProvider = ServiceProvider });
                }
            }
            else if (TestSectionResult.SectionResults != null && TestSectionResult.SectionResults.Any())
            {
                foreach (var sectionResult in TestSectionResult.SectionResults)
                {
                    var sectionComposite = new TestSectionResultComposite
                    {
                        Result = sectionResult,
                        Parent = this,
                        ServiceProvider = ServiceProvider
                    };
                    Children.Add(sectionComposite);
                    sectionComposite.GenerateChildren();
                }
            }
        }

        public override async Task LoadTotalScoreData()
        {
            if (TestSectionResult.SectionResults.Count == 0)
            {
                var testSectionQuestionRepository = ServiceProvider.GetService<IRepository<TestSectionQuestion>>();
                var questions = await testSectionQuestionRepository.ReadQueryable.Where(x => x.TestSectionId == TestSectionResult.TestSectionId)
                    .Include(x => x.Question)
                    .Select(x => x.Question)
                    .ToListAsync();

                TestSectionResult.CorrectTotal = questions.Sum(x => x.CorrectTotal);
                TestSectionResult.SkillScores = new List<SkillScores>
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
                foreach (var child in Children.Where(x => x is ResultComposite).Cast<ResultComposite>())
                {
                    await child.LoadTotalScoreData();
                }

                TestSectionResult.CorrectTotal = TestSectionResult.SectionResults.Sum(x => x.CorrectTotal);
                TestSectionResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        TotalCount =  TestSectionResult.SectionResults.Sum(x => x.CorrectTotal),
                        TotalQuestion =  TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.TotalQuestion)
                    }
                };
            }
        }

        public override void Start()
        {
            if (TestSectionResult.Status is EnumResultStatus.New or EnumResultStatus.Unfinished)
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
                if (Children.All(c => c is TestAnswerLeaf ta && ta.TestAnswer.Status == Shared.Enums.EnumAnswerStatus.Done))
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
