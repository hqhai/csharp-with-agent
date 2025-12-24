// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
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
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly IRepository<TestScore> _testScoreRepository;
        private readonly IMediator _mediator;
        private readonly SubmitTestAiSpeakingPublisher _submitTestAiSpeakingPublisher;

        public SpeakingAITestLayoutHandler(
            ITestSectionResultRepository testSectionResultRepository,
            IProsodyScoreRepository prosodyScoreRepository,
            IRepository<TestAnswer> testAnswerRepository,
            IRepository<TestScore> testScoreRepository,
            IMediator mediator,
            SubmitTestAiSpeakingPublisher submitTestAiSpeakingPublisher)
        {
            _testSectionResultRepository = testSectionResultRepository;
            _prosodyScoreRepository = prosodyScoreRepository;
            _testAnswerRepository = testAnswerRepository;
            _testScoreRepository = testScoreRepository;
            _mediator = mediator;
            _submitTestAiSpeakingPublisher = submitTestAiSpeakingPublisher;
        }

        public async Task HandleAsync(TestSectionResult testSectionResult, TestResult testResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(testResult);
            ArgumentNullException.ThrowIfNull(testSectionResult);
            // === copy logic UpdateTestSpeaking(...) của bạn vào đây ===
            var testAnswers = await _testAnswerRepository.Queryable
                .Include(x => x.TestSection)
                .Where(x => x.TestSectionResultId == testSectionResult.Id)
                .ToListAsync(cancellationToken);

            var (questionArray, answerArray, averagePronScore, count) = ExtractQuestionAnswerAndPronunciationScores(testAnswers);
            var scoreRanges = await _prosodyScoreRepository.ReadQueryable.ToListAsync(cancellationToken);

            (long bandScore, string? feedBack) = GetBandScore(averagePronScore, scoreRanges);

            var testScores = new List<TestScore>
            {
                new TestScore
                {
                    TestSectionResultId = testSectionResult.Id,
                    TestResultId = testResult.Id,
                    TestSectionId = testSectionResult.TestSectionId,
                    Score = bandScore,
                    Feedback = feedBack,
                    Criteria = EnumTestScoreCriteria.Pronunciation,
                }
            };

            var criteria = new List<EnumTestScoreCriteria>
            {
                EnumTestScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumTestScoreCriteria.LexicalResource,
                EnumTestScoreCriteria.FluencyAndCoherence
            };

            foreach (var item in criteria)
            {
                var aiResponse = await GetAIResponse(item, questionArray, answerArray, cancellationToken);
                var responseModel = ConvertHelper.Deserialize<AIEvaluationOutputModel>(aiResponse);

                if (long.TryParse(responseModel?.BandScore, out var bandScoreValue))
                {
                    testScores.Add(new TestScore
                    {
                        TestSectionResultId = testSectionResult.Id,
                        TestResultId = testResult.Id,
                        TestSectionId = testSectionResult.TestSectionId,
                        Score = bandScoreValue,
                        Feedback = responseModel?.BandDescriptorText, // nếu model có
                        Criteria = item
                    });
                }
            }

            var score = testScores.Sum(x => x.Score);
            if (testSectionResult.SkillScores == null)
            {
                return;
            }

            // update skillscores giống code bạn
            var skillScores = new List<SkillScores>();
            foreach (var skillScore in testSectionResult.SkillScores)
            {
                if (skillScore.SkillId == testSectionResult.TestSection?.SkillId)
                {
                    skillScore.CorrectCount = score;
                    skillScore.TotalCount = 36;
                    skillScore.Scores = NumberHelper.RoundReduceNumber((double)score / 4);
                }
                skillScores.Add(skillScore);
            }

            testSectionResult.SkillScores = skillScores;
            testSectionResult.CorrectCount += (int)score;

            // websocket
            await SendToWebSocket(testScores, testResult, cancellationToken);

            // persist: bạn đang dùng transaction add scores + bulk update testSectionResult + update testResult
            await SaveTestScoresAsync(testScores, cancellationToken);
            await SaveTestSectionResultAsync(testSectionResult, cancellationToken);

            // nếu cần update testResult ở đây thì inject ITestResultRepository vào handler (tách theo responsibility)
        }

        private async Task SaveTestScoresAsync(List<TestScore> testScores, CancellationToken cancellationToken)
        {
            await _testScoreRepository.ExecuteTransactionAsync(async () =>
            {
                await _testScoreRepository.AddList(testScores);
                await _testScoreRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return new MethodResult<bool>();
            });
        }

        private async Task SaveTestSectionResultAsync(TestSectionResult testSectionResult, CancellationToken cancellationToken)
        {
            await _testSectionResultRepository.BulkUpdateList(new List<TestSectionResult> { testSectionResult }, bulk =>
            {
                bulk.ColumnInputExpression = c => new { c.SkillScoresStr };
            });
        }

        private async Task SendToWebSocket(List<TestScore> scores, TestResult testResult, CancellationToken cancellationToken)
        {
            foreach (var score in scores)
            {
                var model = new SubmitTestAiSpeakingResponseModel
                {
                    CriteriaName = score.Criteria.ToString(),
                    BandScore = score.Score,
                    BandDescriptionText = score.Feedback,
                    TestResultId = testResult.Id,
                };
                await _submitTestAiSpeakingPublisher.Publish(model, cancellationToken);
            }
        }

        private async Task<string> GetAIResponse(
            EnumTestScoreCriteria item,
            IList<string> questionArray,
            IList<string> answerArray,
            CancellationToken cancellationToken)
        {
            string userAiConfig = BuildSpeakingPromptHelper.CustomAnswerConfigToSendGPT(questionArray, answerArray, item);

            var aiResponse = await _mediator.Send(new SubmitAICommand
            {
                SystemRoleAlConfig = BuildSpeakingPromptHelper.GetConfigByType(item, true),
                UserAIConfig = userAiConfig,
                SettingModel = "gpt-4o",
                SettingTemperature = 1,
                SettingFrequecy = 0,
                SettingWordMaxLength = 1000,
                SettingPresence = 0,
                SettingTopP = 1
            }, cancellationToken).ConfigureAwait(false);

            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aiResponse ?? string.Empty);
        }

        public static (long bandScore, string? comment) GetBandScore(double averagePronScore, List<ProsodyScore>? scoreRanges)
        {
            if (scoreRanges == null || scoreRanges.Count == 0)
            {
                return (0, string.Empty);
            }

            foreach (var range in scoreRanges)
            {
                if (averagePronScore >= range.MinScore && averagePronScore <= range.MaxScore)
                {
                    return ((long)range.BandScore, range.BandComment);
                }
            }
            return (0, string.Empty);
        }

        private static (IList<string>, IList<string>, double, int) ExtractQuestionAnswerAndPronunciationScores(IList<TestAnswer>? testAnswers)
        {
            IList<string> questionArray = new List<string>();
            IList<string> answerArray = new List<string>();
            double pronScore = 0;
            int count = 0;

            testAnswers ??= new List<TestAnswer>();
            foreach (var item in testAnswers)
            {
                questionArray.Add(item?.TestSection?.Name ?? string.Empty);
                answerArray.Add(item?.SpeechTextAnswer ?? string.Empty);
                pronScore += item?.PronunciationScore ?? 0;

                if ((item?.PronunciationScore ?? 0) != 0)
                {
                    count++;
                }
            }

            double averagePronScore = Math.Round(count > 0 ? pronScore / count : 0);
            return (questionArray, answerArray, averagePronScore, count);
        }
    }
}
