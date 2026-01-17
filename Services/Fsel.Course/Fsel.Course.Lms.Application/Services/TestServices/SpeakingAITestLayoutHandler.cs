// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers.Test;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Models;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class SpeakingAITestLayoutHandler : ISpeakingAITestLayoutHandler
    {
        private const int MaxCorrect = 36;

        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly IRepository<TestScore> _testScoreRepository;
        private readonly IMediator _mediator;
        private readonly SubmitTestAiSpeakingPublisher _publisher;
        private readonly ITestResultRepository _testResultRepository;

        public SpeakingAITestLayoutHandler(
            ITestSectionResultRepository testSectionResultRepository,
            IProsodyScoreRepository prosodyScoreRepository,
            IRepository<TestAnswer> testAnswerRepository,
            IRepository<TestScore> testScoreRepository,
            IMediator mediator,
            SubmitTestAiSpeakingPublisher publisher,
            ITestResultRepository testResultRepository)
        {
            _testSectionResultRepository = testSectionResultRepository;
            _prosodyScoreRepository = prosodyScoreRepository;
            _testAnswerRepository = testAnswerRepository;
            _testScoreRepository = testScoreRepository;
            _mediator = mediator;
            _publisher = publisher;
            _testResultRepository = testResultRepository;
        }

        #region Entry

        public async Task HandleAsync(TestSectionResult parentSkillSection, TestResult testResult, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(parentSkillSection);
            ArgumentNullException.ThrowIfNull(testResult);

            var children = await LoadChildrenAsync(parentSkillSection.Id, ct);
            if (!children.Any())
            {
                ResetSection(parentSkillSection);
                await PersistAsync(parentSkillSection, children, new List<TestScore>(), ct);
                return;
            }

            var allScores = new List<TestScore>();

            foreach (var child in children)
            {
                var answers = await LoadAnswersAsync(child.Id, ct);
                if (!answers.Any())
                {
                    ResetSection(child);
                    continue;
                }

                var input = BuildSpeakingInput(answers);

                var childScores = await BuildScoresForChildAsync(
                    child,
                    testResult,
                    input,
                    ct);

                ApplyScoresToChild(child, testResult, childScores);
                allScores.AddRange(childScores);
            }

            ApplyScoresToParent(parentSkillSection, testResult, children);
            await PersistAsync(parentSkillSection, children, allScores, ct);
            await PublishAsync(allScores, testResult, ct);
            await UpdateTestResultIfDoneAsync(testResult, ct);
        }

        #endregion Entry

        #region Load data

        private async Task<List<TestSectionResult>> LoadChildrenAsync(Guid parentId, CancellationToken ct)
        {
            return await _testSectionResultRepository.Queryable
                .Where(x => x.ParentTestSectionResultId == parentId)
                .Include(x => x.TestSection)
                .ThenInclude(x => x.Skill)
                .ToListAsync(ct);
        }

        private async Task UpdateTestResultIfDoneAsync(TestResult testResult, CancellationToken cancellationToken)
        {
            if (testResult.Status != EnumResultStatus.Done)
            {
                return;
            }

            // Load tất cả TestSectionResult gốc (không phải con) của TestResult này
            var rootSectionResults = await _testSectionResultRepository.ReadQueryable
                .Where(x => x.TestResultId == testResult.Id)
                .Where(x => !x.ParentTestSectionResultId.HasValue)
                .ToListAsync(cancellationToken);

            if (rootSectionResults.Count == 0)
            {
                return;
            }

            // =========================
            // Tổng hợp SkillScores từ các section results
            // =========================
            var mergedSkillScores = MergeSkillScoresFromSectionResults(rootSectionResults);

            testResult.SkillScores = mergedSkillScores;
            testResult.CorrectCount = (int)mergedSkillScores.Sum(x => x.CorrectCount);
            testResult.CorrectTotal = (int)mergedSkillScores.Sum(x => x.TotalCount);

            // =========================
            // Percent (nếu có)
            // =========================
            if (testResult.Test?.ScoringFormulaType == EnumScoringFormulaType.Percent)
            {
                testResult.PercentModule = rootSectionResults.Sum(x => x.PercentModule);
            }
            else if (testResult.Test?.ScoringFormulaType == EnumScoringFormulaType.BandScore)
            {
                testResult.Score = rootSectionResults.Sum(x => x.ScoreModule);
            }

            // Persist TestResult
            await _testResultRepository.BulkUpdateList(
                new List<TestResult> { testResult },
                bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new
                    {
                        c.StudentId,
                        c.TestGroupResultId
                    };
                });

            await _testResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static List<SkillScores> MergeSkillScoresFromSectionResults(List<TestSectionResult> sectionResults)
        {
            var dict = new Dictionary<Guid, SkillScores>();

            foreach (var sr in sectionResults)
            {
                if (sr.SkillScores == null)
                {
                    continue;
                }

                foreach (var s in sr.SkillScores)
                {
                    if (!s.SkillId.HasValue)
                    {
                        continue;
                    }

                    if (!dict.TryGetValue(s.SkillId.Value, out var existed))
                    {
                        existed = new SkillScores
                        {
                            SkillId = s.SkillId,
                            SkillName = s.SkillName,
                            SkillFilePath = s.SkillFilePath,
                        };
                        dict.Add(s.SkillId.Value, existed);
                    }

                    // Weighted average score theo TotalCount
                    var totalBefore = existed.TotalCount;
                    var totalAfter = totalBefore + s.TotalCount;

                    if (totalAfter > 0)
                    {
                        existed.Scores =
                            ((existed.Scores * totalBefore) + (s.Scores * s.TotalCount))
                            / totalAfter;
                    }

                    existed.CorrectCount += s.CorrectCount;
                    existed.TotalCount += s.TotalCount;
                }
            }

            foreach (var skill in dict.Values)
            {
                skill.Scores = NumberHelper.RoundReduceNumber(skill.Scores);
            }

            return dict.Values.ToList();
        }

        private async Task<List<TestAnswer>> LoadAnswersAsync(Guid childId, CancellationToken ct)
        {
            return await _testAnswerRepository.Queryable
                .Include(x => x.TestSection)
                .Where(x => x.TestSectionResultId == childId)
                .ToListAsync(ct);
        }

        #endregion Load data

        #region Build input

        private static SpeakingEvaluationInput BuildSpeakingInput(IList<TestAnswer> answers)
        {
            var questions = new List<string>();
            var responses = new List<string>();

            double totalPron = 0;
            int count = 0;

            foreach (var a in answers)
            {
                questions.Add(a.TestSection?.Name ?? string.Empty);
                responses.Add(a.SpeechTextAnswer ?? string.Empty);

                if ((a.PronunciationScore ?? 0) > 0)
                {
                    totalPron += a.PronunciationScore!.Value;
                    count++;
                }
            }

            return new SpeakingEvaluationInput
            {
                Questions = questions,
                Answers = responses,
                AveragePronunciationScore = Math.Round(count > 0 ? totalPron / count : 0)
            };
        }

        private sealed class SpeakingEvaluationInput
        {
            public IList<string> Questions { get; init; } = new List<string>();
            public IList<string> Answers { get; init; } = new List<string>();
            public double AveragePronunciationScore { get; init; }
        }

        #endregion Build input

        #region AI scoring (PER CHILD)

        private async Task<List<TestScore>> BuildScoresForChildAsync(
            TestSectionResult child,
            TestResult testResult,
            SpeakingEvaluationInput input,
            CancellationToken ct)
        {
            var scores = new List<TestScore>();

            var ranges = await _prosodyScoreRepository.ReadQueryable.ToListAsync(ct);
            var (band, comment) = GetBandScore(input.AveragePronunciationScore, ranges);

            scores.Add(CreateScore(child, testResult, EnumTestScoreCriteria.Pronunciation, band, comment));

            foreach (var criteria in new[]
            {
                EnumTestScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumTestScoreCriteria.LexicalResource,
                EnumTestScoreCriteria.FluencyAndCoherence
            })
            {
                var ai = await GetAIResponseAsync(criteria, input, ct);
                var model = ConvertHelper.Deserialize<AIEvaluationOutputModel>(ai);

                if (long.TryParse(model?.BandScore, out var score))
                {
                    scores.Add(CreateScore(
                        child,
                        testResult,
                        criteria,
                        score,
                        model?.BandDescriptorText));
                }
            }

            return scores;
        }

        private static TestScore CreateScore(
            TestSectionResult section,
            TestResult testResult,
            EnumTestScoreCriteria criteria,
            long score,
            string? feedback)
        {
            return new TestScore
            {
                TestSectionResultId = section.Id,
                TestResultId = testResult.Id,
                TestSectionId = section.TestSectionId,
                Criteria = criteria,
                Score = score,
                Feedback = feedback
            };
        }

        #endregion AI scoring (PER CHILD)

        #region Apply scores

        private static void ApplyScoresToChild(
            TestSectionResult child,
            TestResult testResult,
            List<TestScore> scores)
        {
            var scoringFormulaType = testResult.Test?.ScoringFormulaType;
            var total = scores.Sum(x => x.Score);
            child.CorrectCount = (int)total;
            child.CorrectTotal = MaxCorrect;
            child.SkillScores = BuildSkillScores(child.TestSection, (int)total, MaxCorrect);
            if (scoringFormulaType == EnumScoringFormulaType.Percent)
            {
                var percent = child.TestSection?.Percent ?? default;
                child.PercentModule = NumberHelper.ConvertDoublePercent(child.Percent * percent, 2);
            }
            else if (scoringFormulaType == EnumScoringFormulaType.BandScore)
            {
                var score = child.SkillScores[0].Scores;
                var percent = child.TestSection?.Percent ?? default;
                child.ScoreModule = NumberHelper.ConvertDoublePercent(score * percent, 2);
            }
        }

        private static void ApplyScoresToParent(
            TestSectionResult parent,
            TestResult testResult,
            IList<TestSectionResult> children)
        {
            parent.CorrectCount = children.Sum(x => x.CorrectCount);
            parent.CorrectTotal = children.Sum(x => x.CorrectTotal);
            parent.SkillScores = BuildSkillScores(
                parent.TestSection,
                parent.CorrectCount,
                parent.CorrectTotal);

            var scoringFormulaType = testResult.Test?.ScoringFormulaType;
            if (scoringFormulaType == EnumScoringFormulaType.Percent)
            {
                var percent = parent.TestSection?.Percent ?? default;
                parent.PercentModule = NumberHelper.ConvertDoublePercent(children.Sum(x => x.PercentModule) * percent, 2);
            }
            else if (scoringFormulaType == EnumScoringFormulaType.BandScore)
            {
                var scores = parent.SkillScores[0].Scores;
                var percent = parent.TestSection?.Percent ?? default;
                parent.ScoreModule = NumberHelper.ConvertDoublePercent(scores * percent, 2);
            }
        }

        private static void ResetSection(TestSectionResult section)
        {
            section.CorrectCount = 0;
            section.CorrectTotal = MaxCorrect;
            section.SkillScores?.Clear();
        }

        private static List<SkillScores> BuildSkillScores(
            TestSection? testSection,
            int correct,
            int total)
        {
            return new List<SkillScores>
            {
                new SkillScores
                {
                    SkillId = testSection?.SkillId,
                    SkillFilePath = testSection?.Skill?.FilePath,
                    SkillName = testSection?.Skill?.Name,
                    CorrectCount = correct,
                    TotalCount = total,
                    Scores = NumberHelper.RoundReduceNumber((double)correct / 4)
                }
            };
        }

        #endregion Apply scores

        #region Persist & Publish

        private async Task PersistAsync(
            TestSectionResult parent,
            IList<TestSectionResult> children,
            List<TestScore> scores,
            CancellationToken ct)
        {
            await _testScoreRepository.ExecuteTransactionAsync(async () =>
            {
                if (scores.Any())
                {
                    await _testScoreRepository.AddList(scores);
                    await _testScoreRepository.UnitOfWork.SaveChangesAsync(ct);
                }
                return new MethodResult<bool>();
            });

            await _testSectionResultRepository.BulkUpdateList(children.Append(parent).ToList(),
            bulk => bulk.ColumnInputExpression = c => new
            {
                c.SkillScoresStr,
                c.CorrectCount,
                c.CorrectTotal,
                c.PercentModule,
                c.ScoreModule
            });
        }

        private async Task PublishAsync(
            IEnumerable<TestScore> scores,
            TestResult testResult,
            CancellationToken ct)
        {
            foreach (var s in scores)
            {
                await _publisher.Publish(
                    new SubmitTestAiSpeakingResponseModel
                    {
                        CriteriaName = s.Criteria.ToString(),
                        BandScore = s.Score,
                        BandDescriptionText = s.Feedback,
                        TestResultId = testResult.Id
                    }, ct);
            }
        }

        #endregion Persist & Publish

        #region AI helpers

        private async Task<string> GetAIResponseAsync(
            EnumTestScoreCriteria criteria,
            SpeakingEvaluationInput input,
            CancellationToken ct)
        {
            var userConfig = BuildSpeakingPromptHelper
                .CustomAnswerConfigToSendGPT(input.Questions, input.Answers, criteria);

            var response = await _mediator.Send(new SubmitAICommand
            {
                SystemRoleAlConfig = BuildSpeakingPromptHelper.GetConfigByType(criteria, true),
                UserAIConfig = userConfig,
                SettingModel = "gpt-4o",
                SettingTemperature = 1,
                SettingTopP = 1,
                SettingPresence = 0,
                SettingFrequecy = 0,
                SettingWordMaxLength = 1000
            }, ct);

            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(response ?? string.Empty);
        }

        public static (long bandScore, string? comment) GetBandScore(
            double averagePronScore,
            List<ProsodyScore>? ranges)
        {
            if (ranges == null || ranges.Count == 0)
                return (0, string.Empty);

            foreach (var r in ranges)
            {
                if (averagePronScore >= r.MinScore &&
                    averagePronScore <= r.MaxScore)
                {
                    return ((long)r.BandScore, r.BandComment);
                }
            }

            return (0, string.Empty);
        }

        #endregion AI helpers
    }
}
