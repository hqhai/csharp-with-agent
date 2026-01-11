// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Collections.Generic;
    using Domain.Models.EntityModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class TestSectionResultComposite : ResultComposite
    {
        public TestSectionResult TestSectionResult => (TestSectionResult)Result;

        public TestSection? TestSection { get; set; }

        public override BaseTestStateModel ExportState()
        {
            var sectionState = new SectionStateModel
            {
                SectionId = TestSectionResult.TestSectionId,
                SectionResultId = TestSectionResult.Id,
                TestLayoutType = TestSection?.LayoutType,
                Status = TestSectionResult.Status,
                CorrectCount = TestSectionResult.CorrectCount,
                TotalCount = TestSectionResult.SkillScores?.Sum(x => x.TotalCount) ?? default,
                WorkingTime = TestSectionResult.WorkingTime,
                Children = new List<BaseTestStateModel>(),
                CurrentSectionTimeCodeId = TestSectionResult.CurrentSectionTimeCodeId,
                HighestStreak = TestSectionResult.HighestStreak,
                CorrectTotal = TestSectionResult.CorrectTotal,
                PercentResult = TestSectionResult.Percent,
                SkillScores = TestSectionResult.SkillScores,
                ScoreModule = TestSectionResult.ScoreModule,
                FilePath = TestSection?.Skill?.FilePath,
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
                TestLayoutType = TestSectionResult.TestSection?.LayoutType,
                CorrectCount = TestSectionResult.CorrectCount,
                TotalCount = TestSectionResult.SkillScores?.Sum(x => x.TotalCount) ?? default,
                WorkingTime = TestSectionResult.WorkingTime,
                Children = new List<BaseTestStateModel>(),
                FilePath = TestSection?.Skill?.FilePath,
                CorrectTotal = TestSectionResult.CorrectTotal,
                CurrentSectionTimeCodeId = TestSectionResult.CurrentSectionTimeCodeId,
                HighestStreak = TestSectionResult.HighestStreak,
                PercentResult = TestSectionResult.Percent,
                ScoreModule = TestSectionResult.ScoreModule,
                SkillScores = TestSectionResult.SkillScores,
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
                    var questionModel = new QuestionStateModel { QuestionId = x.QuestionId, DisplayOrder = TestSectionResult.TestSection.DisplayOrder };
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
                        CorrectQuestion = TestSectionResult.SectionResults.Where(x=>x.SkillScores != null && x.SkillScores.Any())
                                                           .SelectMany(x => x.SkillScores)
                                                           .Sum(x => x.CorrectQuestion),
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

        public override async Task Submit(SubmitContext context)
        {
            await base.Submit(context);

            if (Children.Count > 0)
            {
                var scoringFormulaConfigs = (TestSection?.ScoringFormulaConfigs ?? new List<ScoringFormulaConfig>()).ToList();

                if (Children.All(c => c is TestAnswerLeaf ta && ta.TestAnswer.Status == EnumAnswerStatus.Done))
                {
                    TestSectionResult.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                    var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);
                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        firstSkillScore.CountQuestion = TestSectionResult.TestAnswers.Count;
                        firstSkillScore.CorrectQuestion = TestSectionResult.TestAnswers.Count(x => x.IsCorrect == true);
                        firstSkillScore.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                        firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                        firstSkillScore.SkillName = TestSectionResult.TestSection?.Skill?.Name;
                        firstSkillScore.Scores = scores;
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }

                    if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                    {
                        TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((TestSectionResult.Percent * TestSection.Percent.Value), 2);
                    }
                    if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                    {
                        TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                    }
                }
                else if (Children.All(c => c is TestSectionResultComposite))
                {
                    TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(t => t.CorrectCount);
                    var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);

                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        var skillScores = TestSectionResult.SectionResults.Where(x => x.SkillScores != null).SelectMany(x => x.SkillScores);

                        firstSkillScore.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                        firstSkillScore.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                        firstSkillScore.CorrectQuestion = skillScores.Sum(x => x.CorrectQuestion ?? 0);
                        firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                        firstSkillScore.SkillName = TestSectionResult.TestSection?.Skill?.Name;
                        firstSkillScore.Scores = scores;
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }

                    if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                    {
                        var totelChildPercentModule = TestSectionResult.SectionResults.Sum(x => x.PercentModule);
                        TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((totelChildPercentModule * TestSection.Percent.Value), 2);
                    }
                    if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                    {
                        TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                    }
                }
                else
                {
                    TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(t => t.CorrectCount);
                    var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);

                    // get layout and process
                    if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                    {
                        var firstSkillScore = TestSectionResult.SkillScores.First();
                        var skillScores = TestSectionResult.SectionResults.Where(x => x.SkillScores != null).SelectMany(x => x.SkillScores);

                        firstSkillScore.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                        firstSkillScore.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                        firstSkillScore.CorrectQuestion = skillScores.Sum(x => x.CorrectQuestion ?? 0);
                        firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                        firstSkillScore.SkillName = TestSectionResult.TestSection?.Skill?.Name;
                        firstSkillScore.Scores = GetBandScore(firstSkillScore.CorrectCount, scoringFormulaConfigs);
                        TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                    }
                    if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                    {
                        var totelChildPercentModule = TestSectionResult.SectionResults.Sum(x => x.PercentModule);
                        TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((totelChildPercentModule * TestSection.Percent.Value), 2);
                    }
                    if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                    {
                        TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                    }
                }
            }
            var answers = GetPlainQuestionStates().Select(x => x.Answer != null && x.Answer.IsCorrect == true).ToList();
            TestSectionResult.HighestStreak = answers.GetHighestStreak();
            TestSectionResult.Status = EnumResultStatus.Done;
        }

        public override IEnumerable<QuestionStateModel> GetPlainQuestionStates()
        {
            if (TestSection?.TestSectionQuestions.Any() == true)
            {
                var answerChildren = Children.Where(x => x is TestAnswerLeaf).Select(x => (QuestionStateModel)x.ExportState()).ToList();
                var notAnswerQuestions = new List<QuestionStateModel>();
                foreach (var sectionQuestion in TestSection.TestSectionQuestions)
                {
                    var answer = answerChildren.FirstOrDefault(a => a.QuestionId == sectionQuestion.QuestionId);
                    if (answer != null)
                    {
                        answer.UpdatedDate = sectionQuestion.UpdatedDate ?? sectionQuestion.CreatedDate;
                    }
                    else
                    {
                        notAnswerQuestions.Add(new QuestionStateModel
                        {
                            QuestionId = sectionQuestion.QuestionId,
                            UpdatedDate = sectionQuestion.UpdatedDate ?? sectionQuestion.CreatedDate
                        });
                    }
                }

                return answerChildren.Concat(notAnswerQuestions).OrderBy(x => x.UpdatedDate).ToList();
            }
            else
            {
                return base.GetPlainQuestionStates();
            }
        }

        public override async Task SubmitTest(SubmitContext context)
        {
            await base.SubmitTest(context);

            var scoringFormulaConfigs = (TestSection?.ScoringFormulaConfigs ?? new List<ScoringFormulaConfig>()).ToList();

            if (Children.All(c => c is TestAnswerLeaf ta && ta.TestAnswer.Status == EnumAnswerStatus.Done))
            {
                TestSectionResult.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);

                if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                {
                    var firstSkillScore = TestSectionResult.SkillScores.First();
                    firstSkillScore.CountQuestion = TestSectionResult.TestAnswers.Count;
                    firstSkillScore.CorrectQuestion = TestSectionResult.TestAnswers.Count(x => x.IsCorrect == true);
                    firstSkillScore.CorrectCount = TestSectionResult.TestAnswers.Sum(t => t.CorrectCount);
                    firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                    firstSkillScore.SkillName = TestSection?.Skill?.Name;
                    firstSkillScore.SkillFilePath = TestSection?.Skill?.FilePath;
                    firstSkillScore.Scores = scores;

                    TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }

                if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((TestSectionResult.Percent * TestSection.Percent.Value), 2);
                }
                if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                }
            }
            else if (Children.All(c => c is TestSectionResultComposite))
            {
                TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(t => t.CorrectCount);
                var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);

                if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                {
                    var firstSkillScore = TestSectionResult.SkillScores.First();
                    var skillScores = TestSectionResult.SectionResults.Where(x => x.SkillScores != null).SelectMany(x => x.SkillScores!);

                    firstSkillScore.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                    firstSkillScore.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                    firstSkillScore.CorrectQuestion = skillScores.Sum(x => x.CorrectQuestion ?? 0);
                    firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                    firstSkillScore.SkillName = TestSection?.Skill?.Name;
                    firstSkillScore.SkillFilePath = TestSection?.Skill?.FilePath;
                    firstSkillScore.Scores = scores;
                    TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }

                if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((TestSectionResult.Percent * TestSection.Percent.Value), 2);
                }
                if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                }
            }
            else
            {
                TestSectionResult.CorrectCount = TestSectionResult.SectionResults.Sum(t => t.CorrectCount);
                var scores = GetBandScore(TestSectionResult.CorrectCount, scoringFormulaConfigs);

                // get layout and process
                if (TestSectionResult.SkillScores != null && TestSectionResult.SkillScores.Any())
                {
                    var firstSkillScore = TestSectionResult.SkillScores.First();
                    var skillScores = TestSectionResult.SectionResults.Where(x => x.SkillScores != null).SelectMany(x => x.SkillScores);

                    firstSkillScore.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                    firstSkillScore.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                    firstSkillScore.CorrectQuestion = skillScores.Sum(x => x.CorrectQuestion ?? 0);
                    firstSkillScore.SkillId = TestSectionResult.TestSection?.SkillId;
                    firstSkillScore.SkillName = TestSection?.Skill?.Name;
                    firstSkillScore.SkillFilePath = TestSection?.Skill?.FilePath;
                    firstSkillScore.Scores = scores;
                    TestSectionResult.SkillScores = new List<SkillScores> { firstSkillScore };
                }
                if (context.ScoringFormulaType == EnumScoringFormulaType.Percent && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.PercentModule = NumberHelper.ConvertDoublePercent((TestSectionResult.Percent * TestSection.Percent.Value), 2);
                }
                if (context.ScoringFormulaType == EnumScoringFormulaType.BandScore && TestSection != null && TestSection.Percent.HasValue)
                {
                    TestSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * TestSection.Percent.Value, 2);
                }
            }

            var answers = GetPlainQuestionStates().Select(x => x.Answer != null && x.Answer.IsCorrect == true).ToList();
            TestSectionResult.HighestStreak = answers.GetHighestStreak();
            TestSectionResult.Status = EnumResultStatus.Done;
        }

        public static double GetBandScore(double correctAnswers, IReadOnlyCollection<ScoringFormulaConfig> configs)
        {
            if (configs == null || configs.Count == 0)
            {
                return default;
            }

            // Không clamp cứng – chỉ đảm bảo không âm
            if (correctAnswers < 0)
            {
                correctAnswers = 0;
            }

            var rule = configs
                .OrderByDescending(x => x.From)
                .FirstOrDefault(x => correctAnswers >= x.From);

            return rule?.Equal ?? default;
        }

        public override async Task LoadTestHierarchicalData()
        {
            if (TestSection?.TestSections != null && TestSection.TestSections.Any() && Children != null && Children.Any())
            {
                foreach (var child in Children)
                {
                    if (child is TestSectionResultComposite testSectionResultComposite)
                    {
                        var testSection = TestSection.TestSections.FirstOrDefault(x => x.Id == testSectionResultComposite.TestSectionResult.TestSectionId);
                        if (testSection != null)
                        {
                            testSectionResultComposite.TestSection = testSection;
                            await testSectionResultComposite.LoadTestHierarchicalData();
                        }
                    }
                }

                Children = Children.Where(x => x is TestSectionResultComposite)
                    .Cast<TestSectionResultComposite>()
                    .OrderBy(x => x.TestSection?.DisplayOrder)
                    .Select(x => x as ResultComponent).ToList();
            }
        }
    }
}
