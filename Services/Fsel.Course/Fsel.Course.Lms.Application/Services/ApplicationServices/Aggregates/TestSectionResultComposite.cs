// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Domain.Models.EntityModels;
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
                CorrectCount = TestSectionResult.CorrectCount,
                TotalCount = TestSectionResult.SkillScores.Sum(x => x.TotalCount),
                WorkingTime = TestSectionResult.WorkingTime,
                Children = new List<BaseTestStateModel>(),
                UpdatedDate = TestSectionResult?.UpdatedDate ?? TestSectionResult?.CreatedDate
            };
            if (Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    sectionState.Children.Add(child.ExportState());
                }
            }
            return sectionState;
        }
        public override BaseTestStateModel ExportForTestState()
        {
            var sectionState = new SectionStateModel
            {
                SectionId = TestSectionResult.TestSectionId,
                SectionResultId = TestSectionResult.Id,
                Status = TestSectionResult.Status,
                TestLayoutType = TestSectionResult.TestSection.LayoutType,
                CorrectCount = TestSectionResult.CorrectCount,
                TotalCount = TestSectionResult.SkillScores.Sum(x => x.TotalCount),
                WorkingTime = TestSectionResult.WorkingTime,
                Children = new List<BaseTestStateModel>(),
                UpdatedDate = TestSectionResult?.UpdatedDate ?? TestSectionResult?.CreatedDate
            };
            if (Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    sectionState.Children.Add(child.ExportForTestState());
                }
            }
            else if (TestSectionResult.TestSection?.TestSectionQuestions?.Any() == true)
            {
                sectionState.Children = TestSectionResult.TestSection.TestSectionQuestions.Select(BaseTestStateModel (x) =>
                {
                    var questionModel = new QuestionStateModel { QuestionId = x.QuestionId };
                    var testAnswer = TestSectionResult?.TestAnswers.FirstOrDefault(t => t.QuestionId == x.QuestionId);
                    questionModel.TestAnswerId = testAnswer?.Id;
                    if (testAnswer != null)
                    {
                        questionModel.Answer = new AnswerModel() { Answer = testAnswer.Answer, CorrectCount = testAnswer.CorrectCount, IsCorrect = testAnswer.IsCorrect, };
                    }

                    return questionModel;
                }).ToList();
            }

            return sectionState;
        }
        public override void GenerateChildren()
        {
            if (TestSectionResult.TestAnswers.Any())
            {
                foreach (var testAnswer in TestSectionResult.TestAnswers)
                {
                    Children.Add(new TestAnswerLeaf { Result = testAnswer, Parent = this, ServiceProvider = ServiceProvider });
                }
            }
            else if (TestSectionResult.SectionResults.Any())
            {
                foreach (var sectionResult in TestSectionResult.SectionResults)
                {
                    var sectionComposite = new TestSectionResultComposite { Result = sectionResult, Parent = this, ServiceProvider = ServiceProvider };
                    Children.Add(sectionComposite);
                    sectionComposite.GenerateChildren();
                }
            }
        }

        public override async Task LoadTotalScoreData()
        {
            if (TestSectionResult.SectionResults.Count == 0)
            {
                var testSectionQuestionRepository = ServiceProvider.GetRequiredService<IRepository<TestSectionQuestion>>();
                var questions = await testSectionQuestionRepository.ReadQueryable.Where(x => x.TestSectionId == TestSectionResult.TestSectionId)
                    .Include(x => x.Question)
                    .Select(x => x.Question)
                    .ToListAsync();

                TestSectionResult.CorrectTotal = questions.Sum(x => x.CorrectTotal);
                TestSectionResult.SkillScores = new List<SkillScores> { new SkillScores { TotalCount = questions.Sum(x => x.CorrectTotal), TotalQuestion = questions.Count } };
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
                        TotalCount = TestSectionResult.SectionResults.Sum(x => x.CorrectTotal),
                        TotalQuestion = TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.TotalQuestion)
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

        public override async Task Submit(Guid id)
        {
            await base.Submit(id);

            if (Children.Count > 0)
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
                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        firstSkillScore.CountQuestion = TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CountQuestion);
                        firstSkillScore.CorrectCount = TestSectionResult.SectionResults.SelectMany(x => x.SkillScores).Sum(x => x.CorrectCount);
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }

                    TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(x => x.CorrectCount);
                }

                TestSectionResult.Status = EnumResultStatus.Done;
            }
        }
    }
}
