// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Lms.Application.Commands.AiCmd;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class SpeakingAIService : ISpeakingAIService
    {
        private readonly IMediator _mediator;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeScoreRepository _examPracticeScoreRepository;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly SubmitAiSpeakingAnswerPublisher _submitAiSpeakingAnswerPublisher;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IExamPracticeAISettingRepository _examPracticeAISettingRepository;
        private readonly IMapper _mapper;
        private const int MaxScoreAI = 10;

        public SpeakingAIService(IMediator mediator, IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeScoreRepository examPracticeScoreRepository,
            IProsodyScoreRepository prosodyScoreRepository,
            SubmitAiSpeakingAnswerPublisher submitAiSpeakingAnswerPublisher,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IExamPracticeAISettingRepository examPracticeAISettingRepository,
            IMapper mapper)
        {
            _mediator = mediator;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeScoreRepository = examPracticeScoreRepository;
            _prosodyScoreRepository = prosodyScoreRepository;
            _submitAiSpeakingAnswerPublisher = submitAiSpeakingAnswerPublisher;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _examPracticeAISettingRepository = examPracticeAISettingRepository;
            _mapper = mapper;
        }

        #region Handle

        public async Task<bool> EvaluationSpeakingAI(Guid examPracticeResultId, Guid examPracticeSectionId, CancellationToken cancellationToken)
        {
            var examPracticeResult = await GetExamPracticeResultWithDetails(examPracticeResultId, cancellationToken);
            if (examPracticeResult?.ExamPractice == null)
            {
                return false;
            }

            var type = examPracticeResult.ExamPractice.Type;
            var (questionArray, answerArray, avgPronScore, avgFluencyScore, count) = ExtractQuestionAnswerAndPronunciationScores(examPracticeResult);
            var scoreRanges = await _prosodyScoreRepository.Queryable
                                                .Where(x => x.ModuleAIType == EnumExamPracticeModuleAIType.Speaking)
                                                .ToListAsync(cancellationToken);

            var examPracticeSection = await _examPracticeSectionRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeSections)
                                                                          .FirstOrDefaultAsync(x => x.Id == examPracticeSectionId, cancellationToken);
            var examPracticeSectionResult = await GetExamPracticeSectionResult(examPracticeSectionId, examPracticeResultId, cancellationToken);
            if (examPracticeSectionResult?.SkillScores == null)
            {
                return false;
            }
            var skillScoreChirldren = BuildSpeakingAiScores(avgPronScore, avgFluencyScore, count).ToList();

            var examPracticeScores = new List<ExamPracticeScore>();
            List<EnumExamPracticeScoreCriteria> criteria;
            long totalScore = 0;
            var skillScores = new List<SkillScores>();
            switch (type)
            {
                case EnumExamPracticeType.IELTS:
                {
                    var (bandScore, feedBackUs, feedBack) = GetBandScore(avgFluencyScore, scoreRanges);
                    criteria = GetEvaluationCriteria();

                    var examPracticeScorePronun = CreateExamPracticeScore(EnumExamPracticeScoreCriteria.Pronunciation, bandScore, feedBackUs ?? string.Empty, string.Empty, examPracticeSectionId, examPracticeResultId);
                    examPracticeScores.Add(examPracticeScorePronun);

                    await AddAIScoresToExamPracticeScores(criteria, questionArray, answerArray, skillScoreChirldren, scoreRanges, examPracticeScores, examPracticeSection, examPracticeSectionResult, cancellationToken);
                    totalScore = examPracticeScores.Sum(x => x.Score);
                    skillScores = UpdateSkillScoresIelts(examPracticeSectionResult.SkillScores, examPracticeSection?.CourseSkill, totalScore);
                    break;
                }
                default:
                {
                    var (proResponse, bandScorePron, commentPron) = GetExamPracticeAIGradingLanguages(avgPronScore, EnumExamPracticeAIType.Pronunciation, scoreRanges);
                    var (fluencyResponse, bandScoreFluency, commentFluency) = GetExamPracticeAIGradingLanguages(avgFluencyScore, EnumExamPracticeAIType.Fluency, scoreRanges);

                    var examPracticeScorePronun = CreateExamPracticeScore(EnumExamPracticeScoreCriteria.Pronunciation, bandScorePron, commentPron, proResponse.Serialize(), examPracticeSectionId, examPracticeResultId);
                    var examPracticeScoreFluency = CreateExamPracticeScore(EnumExamPracticeScoreCriteria.FluencyAndCoherence, bandScoreFluency, commentFluency, fluencyResponse.Serialize(), examPracticeSectionId, examPracticeResultId);
                    examPracticeScores.AddRange(new[] { examPracticeScorePronun, examPracticeScoreFluency });

                    criteria = GetEvaluationVstepCriteria();
                    await AddAIScoresToExamPracticeScores(criteria, questionArray, answerArray, skillScoreChirldren, scoreRanges, examPracticeScores, examPracticeSection, examPracticeSectionResult, cancellationToken);

                    totalScore = examPracticeScores.Sum(x => x.Score);
                    skillScores = UpdateSkillScoreVsteps(examPracticeSectionResult.SkillScores, examPracticeSection?.CourseSkill, totalScore);
                    break;
                }
            }

            examPracticeSectionResult.SkillScores = skillScores;
            examPracticeSectionResult.CorrectCount += (int)totalScore;

            UpdateExamPracticeResultSkillScores(examPracticeResult, skillScores);
            await UpdateExamPracticeSectionResultAsync(examPracticeSection, examPracticeScores, examPracticeResult, skillScores, cancellationToken);
            await SendToWebSocket(examPracticeScores, cancellationToken);
            await SaveExamPracticeScoresToDatabase(examPracticeScores);
            await SaveExamPracticeSectionResultToDatabase(examPracticeSectionResult);
            await SaveExamPracticeResultAsync(examPracticeResult);
            return true;
        }

        public static IList<SkillScores> BuildSpeakingAiScores(
        double avgPronScore,
        double avgFluencyScore,
        int questionCount,
        int roundDecimals = 2)
        {
            // Guard clauses & chuẩn hoá
            if (questionCount < 0)
            {
                questionCount = 0;
            }

            // Helper nội bộ để tránh lặp
            SkillScores Create(EnumExamPracticeScoreCriteria criteria, double score) => new SkillScores
            {
                Skill = EnumCourseSkill.Speaking,
                ScoreCriteria = criteria,
                // Giữ nguyên logic bạn đang dùng: CorrectCount = điểm trung bình
                CorrectCount = Math.Round(score, roundDecimals),
                Scores = Math.Round(score, roundDecimals),
                CountQuestion = questionCount,
                TotalQuestion = questionCount,
                TotalCount = MaxScoreAI
            };

            return new List<SkillScores>
            {
                Create(EnumExamPracticeScoreCriteria.Pronunciation, avgPronScore),
                Create(EnumExamPracticeScoreCriteria.FluencyAndCoherence, avgFluencyScore) // <-- tiêu chí đúng
            };
        }

        private async Task<ExamPracticeResult?> GetExamPracticeResultWithDetails(Guid examPracticeResultId, CancellationToken cancellationToken)
        {
            return await _examPracticeResultRepository.Queryable
                .Include(x => x.ExamPracticeAnswers)
                .ThenInclude(x => x.ExamPracticeSection)
                .Include(x => x.ExamPracticeSectionResults)
                .Include(x => x.ExamPractice)
                .FirstOrDefaultAsync(x => x.Id == examPracticeResultId, cancellationToken);
        }

        private static List<EnumExamPracticeScoreCriteria> GetEvaluationCriteria()
        {
            return new List<EnumExamPracticeScoreCriteria>
            {
                EnumExamPracticeScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumExamPracticeScoreCriteria.LexicalResource,
                EnumExamPracticeScoreCriteria.FluencyAndCoherence
            };
        }

        private static List<EnumExamPracticeScoreCriteria> GetEvaluationVstepCriteria()
        {
            return new List<EnumExamPracticeScoreCriteria>
            {
                EnumExamPracticeScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumExamPracticeScoreCriteria.DiscourseManagement,
                EnumExamPracticeScoreCriteria.Vocabulary,
            };
        }

        // Mapping function between EnumMockTestScoreCriteria and EnumMockTestAIType
        private static EnumExamPracticeAIType ConvertScoreCriteriaToAIType(EnumExamPracticeScoreCriteria criteria)
        {
            return criteria switch
            {
                EnumExamPracticeScoreCriteria.GrammaticalRangeAndAccuracy => EnumExamPracticeAIType.Grammar,
                EnumExamPracticeScoreCriteria.LexicalResource => EnumExamPracticeAIType.LexicalResource,
                EnumExamPracticeScoreCriteria.Vocabulary => EnumExamPracticeAIType.Vocabulary,
                EnumExamPracticeScoreCriteria.DiscourseManagement => EnumExamPracticeAIType.DiscourseManagement,
                _ => throw new ArgumentOutOfRangeException(nameof(criteria), criteria, null)
            };
        }

        private async Task<ExamPracticeSectionResult?> GetExamPracticeSectionResult(Guid examPracticeSectionId, Guid examPracticeResultId, CancellationToken cancellationToken)
        {
            return await _examPracticeSectionResultRepository.Queryable
                .Where(x => x.ExamPracticeSectionId == examPracticeSectionId && x.ExamPracticeResultId == examPracticeResultId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task AddAIScoresToExamPracticeScores(
            List<EnumExamPracticeScoreCriteria> criteria,
            List<string> questionArray,
            List<string> answerArray,
            List<SkillScores> skillScores,
            List<ProsodyScore> prosodyScores,
            List<ExamPracticeScore> examPracticeScores,
            ExamPracticeSection? examPracticeSection,
            ExamPracticeSectionResult examPracticeSectionResult,
            CancellationToken cancellationToken)
        {
            var examPracticeSectionId = examPracticeSection?.ExamPracticeSections.FirstOrDefault()?.Id ?? examPracticeSectionResult.ExamPracticeSectionId;

            var examPracticeAISetting = await _examPracticeAISettingRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeAICriteriaSettings)
                                                                              .FirstOrDefaultAsync(x => x.ExamPracticeSectionId == examPracticeSectionId, cancellationToken);

            if (examPracticeAISetting == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(examPracticeAISetting.SystemRoleAlConfig) || examPracticeAISetting.Prompts != null && examPracticeAISetting.Prompts.Count == 0)
            {
                foreach (var item in criteria)
                {
                    var aICriteriaSetting = examPracticeAISetting.ExamPracticeAICriteriaSettings.FirstOrDefault(x => x.CriteriaName == ConvertScoreCriteriaToAIType(item));
                    if (aICriteriaSetting == null)
                    {
                        continue;
                    }
                    string userAiConfig = CustomAnswerConfigToSendGPT(questionArray, answerArray, aICriteriaSetting);
                    var aIResponse = await SendChatGPTSchema(examPracticeAISetting, aICriteriaSetting.SystemRoleAlConfig ?? string.Empty, userAiConfig, aICriteriaSetting.JsonSchema ?? string.Empty, cancellationToken);

                    var responseDataModel = ConvertHelper.Deserialize<ExamPracticeAIGradingDataModel>(aIResponse);
                    var responseModel = _mapper.Map<ExamPracticeAIGradingModel>(responseDataModel);
                    if (long.TryParse(responseModel!.BandScore, out long bandScoreValue))
                    {
                        skillScores.Add(new SkillScores
                        {
                            Skill = EnumCourseSkill.Speaking,
                            CorrectCount = bandScoreValue,
                            TotalCount = MaxScoreAI,
                            ScoreCriteria = item,
                            Scores = bandScoreValue
                        });

                        var relevantProsodyScores = prosodyScores.Where(x => x.AIType == ConvertScoreCriteriaToAIType(item)).ToList();
                        var gradingModels = new List<ExamPracticeAIGradingModel>() { responseModel };
                        var matchedScores = relevantProsodyScores.Where(x => x.MinScore <= bandScoreValue && x.MaxScore >= bandScoreValue && x.AIType == ConvertScoreCriteriaToAIType(item)).OrderBy(x => x.Language).ToList();
                        if (matchedScores.Count > 0)
                        {
                            var mainComment = matchedScores[0].BandComment ?? string.Empty;
                            var altComment = matchedScores.Count > 1 ? matchedScores[1].BandComment ?? string.Empty : mainComment;

                            foreach (var model in gradingModels)
                            {
                                model.BandDescriptorText = mainComment;
                            }
                            var aIResponseTranslate = await GetTranslateAIResponse(aIResponse, cancellationToken);

                            var responseTranslateDataModel = ConvertHelper.Deserialize<ExamPracticeAIGradingDataModel>(aIResponseTranslate);
                            var responseTranslateModel = _mapper.Map<ExamPracticeAIGradingModel>(responseTranslateDataModel);

                            var altData = new List<ExamPracticeAIGradingModel>
                            {
                                new ExamPracticeAIGradingModel
                                {
                                    BandScore = gradingModels[0].BandScore,
                                    BandDescriptorText = altComment,
                                    Explanation = responseTranslateModel?.Explanation ?? string.Empty,
                                    SuggestionsForImprovement = responseTranslateModel?.SuggestionsForImprovement ?? string.Empty
                                }
                            };

                            var languageModels = matchedScores
                                .OrderBy(x => x.Language)
                                .Select(x => new ExamPracticeAIGradingLanguageModel
                                {
                                    Language = x.Language,
                                    ExamPracticeAIGradings = x.Language == LanguageAIModule.English ? gradingModels : altData,
                                })
                                .ToList();

                            if (languageModels.Any())
                            {
                                aIResponse = languageModels.Serialize();
                            }
                        }
                        examPracticeScores.Add(CreateExamPracticeScore(item, bandScoreValue, matchedScores[0]?.BandComment ?? string.Empty, aIResponse, examPracticeSectionResult.ExamPracticeSectionId, examPracticeSectionResult.ExamPracticeResultId));
                    }
                }
            }
            else
            {
                foreach (var item in criteria)
                {
                    string userAiConfig = CustomAnswerConfigToSendGPT(questionArray, answerArray, item);

                    var aIResponse = await GetAIResponse(item, userAiConfig, cancellationToken);
                    var responseModel = ConvertHelper.Deserialize<AIEvaluationOutputModel>(aIResponse);
                    if (long.TryParse(responseModel!.BandScore, out long bandScoreValue))
                    {
                        examPracticeScores.Add(CreateExamPracticeScore(item, bandScoreValue, responseModel.BandDescriptorText ?? string.Empty, aIResponse, examPracticeSectionResult.ExamPracticeSectionId, examPracticeSectionResult.ExamPracticeResultId));
                    }
                }
            }
        }

        private static List<SkillScores> UpdateSkillScoresIelts(
          IList<SkillScores> skillScoresList,
          EnumCourseSkill? courseSkill,
          long score)
          => UpdateSkillScoresInternal(skillScoresList, courseSkill, score, totalCount: 36, divisor: 4);

        private static List<SkillScores> UpdateSkillScoreVsteps(
            IList<SkillScores> skillScoresList,
            EnumCourseSkill? courseSkill,
            long score)
            => UpdateSkillScoresInternal(skillScoresList, courseSkill, score, totalCount: 50, divisor: 5);

        private static List<SkillScores> UpdateSkillScoresInternal(
            IList<SkillScores> skillScoresList,
            EnumCourseSkill? courseSkill,
            long score,
            int totalCount,
            int divisor)
        {
            // Defensive checks
            if (skillScoresList == null || skillScoresList.Count == 0 || !courseSkill.HasValue || divisor <= 0)
            {
                return skillScoresList?.ToList() ?? new List<SkillScores>();
            }

            // (Optional) clamp để tránh lệch biên
            var clampedScore = score < 0 ? 0 : (score > totalCount ? totalCount : score);

            var result = new List<SkillScores>(skillScoresList.Count);

            foreach (var s in skillScoresList)
            {
                if (s.Skill == courseSkill.Value)
                {
                    s.CorrectCount = clampedScore;
                    s.TotalCount = totalCount;
                    s.Scores = NumberHelper.RoundReduceNumber((double)clampedScore / divisor);
                }

                result.Add(s);
            }

            return result;
        }

        private static void UpdateExamPracticeResultSkillScores(ExamPracticeResult examPracticeResult, List<SkillScores> skillScores)
        {
            if (examPracticeResult.SkillScores != null && examPracticeResult.SkillScores.Count > 1)
            {
                if (skillScores.Any())
                {
                    var clonedSkillScores = examPracticeResult.SkillScores
                        .Where(x => x.Skill != EnumCourseSkill.Speaking)
                        .ToList();

                    var skillScoreToAdd = skillScores.Single();
                    clonedSkillScores.Add(skillScoreToAdd);
                    examPracticeResult.CorrectCount += (int)skillScoreToAdd.CorrectCount;
                    examPracticeResult.SkillScores = clonedSkillScores;
                }
            }
            else
            {
                examPracticeResult.CorrectCount = (int)skillScores[0].CorrectCount;
                examPracticeResult.SkillScores = skillScores;
            }
        }

        private static (List<string>, List<string>, double, double, int) ExtractQuestionAnswerAndPronunciationScores(ExamPracticeResult examPracticeResult)
        {
            IList<string> questionArray = new List<string>();
            IList<string> answerArray = new List<string>();
            double pronScore = 0;
            double fluencyScore = 0;
            int count = 0;

            examPracticeResult.ExamPracticeAnswers = examPracticeResult.ExamPracticeAnswers.Where(x => x.ExamPracticeSectionResult?.SkillScores?.FirstOrDefault()?.Skill == EnumCourseSkill.Speaking).ToList();

            foreach (var item in examPracticeResult.ExamPracticeAnswers)
            {
                questionArray.Add(item?.ExamPracticeSection?.Name ?? string.Empty);
                answerArray.Add(item?.SpeechTextAnswer ?? string.Empty);
                pronScore += item != null && item.PronunciationScore.HasValue ? item.PronunciationScore.Value : ValueDefault;
                fluencyScore += item != null && item.PronunciationAssessmentAnswer != null && item.PronunciationAssessmentAnswer.FluencyScore.HasValue ? item.PronunciationAssessmentAnswer.FluencyScore.Value : default;
                if (item != null && item.PronunciationScore.HasValue && item.PronunciationScore.Value != ValueDefault)
                {
                    count++;
                }
            }

            double averagePronScore = Math.Round(count > ValueDefault ? (double)pronScore / count : ValueDefault);
            double averageFluencyScore = Math.Round(count > ValueDefault ? (double)fluencyScore / count : ValueDefault);
            return (questionArray.ToList(), answerArray.ToList(), averagePronScore, averageFluencyScore, count);
        }

        private static ExamPracticeScore CreateExamPracticeScore(EnumExamPracticeScoreCriteria criteria, long score, string feedback, string gradingAlFeedback, Guid examPracticeSectionId, Guid examPracticeResultId)
        {
            return new ExamPracticeScore
            {
                Criteria = criteria,
                Score = score,
                FeedBack = feedback,
                GradingAlFeedback = gradingAlFeedback,
                ExamPracticeSectionId = examPracticeSectionId,
                ExamPracticeResultId = examPracticeResultId
            };
        }

        private async Task<string> GetAIResponse(EnumExamPracticeScoreCriteria item, string userAiConfig, CancellationToken cancellationToken)
        {
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SystemRoleAlConfig = GetConfigByType(item, true),
                UserAIConfig = userAiConfig,
                SettingModel = "gpt-4o",
                SettingTemperature = 1,
                SettingFrequecy = 0,
                SettingWordMaxLength = 1000,
                SettingPresence = 0,
                SettingTopP = 1
            }, cancellationToken).ConfigureAwait(false);
            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
        }

        private async Task<string> GetTranslateAIResponse(string gradingAlFeedback, CancellationToken cancellationToken)
        {
            var translateAiRole = await File.ReadAllTextAsync(ResourceSettings.TranslateAiRole, cancellationToken);
            var translateAiInstruction = await File.ReadAllTextAsync(ResourceSettings.TranslateAiInstruction, cancellationToken);
            translateAiInstruction = string.Format(CultureInfo.InvariantCulture, translateAiInstruction ?? string.Empty, gradingAlFeedback);

            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SystemRoleAlConfig = translateAiRole,
                UserAIConfig = translateAiInstruction,
                SettingModel = "gpt-4o-mini",
                SettingTemperature = 1,
                SettingFrequecy = 0,
                SettingWordMaxLength = 1000,
                SettingPresence = 0,
                SettingTopP = 1
            }, cancellationToken).ConfigureAwait(false);
            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
        }

        private async Task<string> SendChatGPTSchema(ExamPracticeAISetting aiConfig, string systemRole, string userAiConfig, string jsonSchema, CancellationToken cancellationToken)
        {
            string aIResponse = "";
            if (string.IsNullOrEmpty(userAiConfig))
            {
                return aIResponse;
            }

            var result = await _mediator.Send(new Commands.AiCmd.V1i1.SubmitAICommand
            {
                SettingModel = ChatBotSetup.O4MINI,
                SettingTemperature = aiConfig.SettingTemperature,
                SettingFrequecy = aiConfig.SettingFrequecy,
                SettingWordMaxLength = aiConfig.SettingWordMaxLength,
                SettingPresence = aiConfig.SettingPresence,
                SettingTopP = 1,
                SystemRoleAlConfig = systemRole,
                UserAIConfig = userAiConfig,
                Format = jsonSchema
            }, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result))
            {
                aIResponse = result;
            }
            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse);
        }

        #region SaveData To DB

        private async Task UpdateExamPracticeSectionResultAsync(ExamPracticeSection? examPracticeSection, IList<ExamPracticeScore> examPracticeScores, ExamPracticeResult examPracticeResult, IList<SkillScores> skillScores, CancellationToken cancellationToken)
        {
            var examPracticeSectionChirldren = examPracticeSection?.ExamPracticeSections.FirstOrDefault();
            if (examPracticeSectionChirldren == null || examPracticeSection == null)
            {
                return;
            }
            var examPracticeSectionResult = await GetExamPracticeSectionResult(examPracticeSectionChirldren.Id, examPracticeResult.Id, cancellationToken);
            if (examPracticeSectionResult != null)
            {
                examPracticeSectionResult.Status = EnumResultStatus.Done;
                examPracticeSectionResult.CorrectCount = (int)skillScores[0].CorrectCount;
                examPracticeSectionResult.CorrectTotal = (int)skillScores[0].TotalCount;
                examPracticeSectionResult.SkillScores = examPracticeScores.Select(x => new SkillScores
                {
                    Skill = EnumCourseSkill.Speaking,
                    CorrectCount = x.Score,
                    CountQuestion = skillScores[0].CountQuestion,
                    ScoreCriteria = x.Criteria,
                    TotalCount = MaxScoreAI,
                    TotalQuestion = skillScores[0].TotalQuestion,
                    Scores = x.Score
                }).ToList();
                await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.SkillScoresStr, entity.Status, entity.Percent };
                });
            }
        }

        private async Task SaveExamPracticeScoresToDatabase(List<ExamPracticeScore> examPracticeScores)
        {
            await _examPracticeScoreRepository.ExecuteTransactionAsync(async () =>
            {
                await _examPracticeScoreRepository.BulkMergeAsync(examPracticeScores);
                return new MethodResult<bool>();
            });
        }

        private async Task SaveExamPracticeSectionResultToDatabase(ExamPracticeSectionResult examPracticeSectionResult)
        {
            try
            {
                await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.Percent, entity.SkillScoresStr };
                });
            }
            catch { }
        }

        private async Task SaveExamPracticeResultAsync(ExamPracticeResult examPracticeResult)
        {
            try
            {
                await _examPracticeResultRepository.BulkUpdateList(new List<ExamPracticeResult> { examPracticeResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.Percent, entity.SkillScoresStr };
                });
            }
            catch { }
        }

        #endregion SaveData To DB

        #endregion Handle

        #region Func

        /// <summary>
        /// Gửi kết quả đển websocket
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendToWebSocket(List<ExamPracticeScore> scores, CancellationToken cancellationToken)
        {
            foreach (var score in scores)
            {
                SubmitExamPracticeAiSpeakingResponseModel model = new SubmitExamPracticeAiSpeakingResponseModel()
                {
                    CriteriaName = score.Criteria.ToString(),
                    BandScore = score.Score,
                    ExamPracticeResultId = score.ExamPracticeResultId,
                    DisplayOrder = scores.IndexOf(score),
                    ExamPracticeAIGradingLanguages = score.GradingAlFeedbacks ?? new List<ExamPracticeAIGradingLanguageModel>()
                };
                await _submitAiSpeakingAnswerPublisher.Publish(model, cancellationToken);
            }
        }

        /// <summary>
        /// Lấy giá trị BandScore theo khoảng.
        /// </summary>
        /// <param name="averagePronScore"></param>
        /// <param name="scoreRanges"></param>
        /// <returns></returns>
        public static (long bandScore, string? commentUs, string? comment) GetBandScore(double averagePronScore, IList<ProsodyScore>? scoreRanges)
        {
            if (scoreRanges == null || scoreRanges.Count == 0)
            {
                return (0, string.Empty, string.Empty);
            }
            var scoreDatas = scoreRanges.Where(x => x.MinScore <= averagePronScore && x.MaxScore >= averagePronScore).OrderBy(x => x.Language).ToList();
            if (scoreDatas.Any())
            {
                return ((long)scoreDatas[0].BandScore, scoreDatas[0].BandComment, scoreDatas[1].BandComment);
            }

            return (0, string.Empty, string.Empty); // Hoặc giá trị mặc định nếu không tìm thấy khoảng phù hợp
        }

        public static (IList<ExamPracticeAIGradingLanguageModel>, long, string) GetExamPracticeAIGradingLanguages(double averagePronScore, EnumExamPracticeAIType practiceAIType, IList<ProsodyScore>? scoreRanges)
        {
            if (scoreRanges == null || scoreRanges.Count == 0)
            {
                return (new List<ExamPracticeAIGradingLanguageModel>(), default, string.Empty);
            }
            var scoreDatas = scoreRanges.Where(x => x.MinScore <= averagePronScore && x.MaxScore >= averagePronScore && x.AIType == practiceAIType).OrderBy(x => x.Language).ToList();
            if (scoreDatas.Any())
            {
                var aIGradingLanguages = new List<ExamPracticeAIGradingLanguageModel>
                {
                    new ExamPracticeAIGradingLanguageModel
                    {
                        Language = scoreDatas[0].Language,
                        ExamPracticeAIGradings = new List<ExamPracticeAIGradingModel>
                        {
                            new ExamPracticeAIGradingModel
                            {
                                BandScore =  scoreDatas[0].BandScore.ToString(CultureInfo.InvariantCulture),
                                BandDescriptorText = scoreDatas[0].BandComment ,
                            }
                        }
                    },
                    new ExamPracticeAIGradingLanguageModel
                    {
                       Language = scoreDatas[1].Language,
                       ExamPracticeAIGradings = new List<ExamPracticeAIGradingModel>
                       {
                           new ExamPracticeAIGradingModel
                           {
                               BandScore =  scoreDatas[1].BandScore.ToString(CultureInfo.InvariantCulture),
                               BandDescriptorText = scoreDatas[1].BandComment ,
                           }
                       }
                    }
                };

                return (aIGradingLanguages, (long)scoreDatas[0].BandScore, scoreDatas[0].BandComment ?? string.Empty);
            }
            return (new List<ExamPracticeAIGradingLanguageModel>(), default, string.Empty);
        }

        /// <summary>
        /// config answer , question. để tạo thành prompt gửi cho chatgpt
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private static string CustomAnswerConfigToSendGPT(IList<string> questions, IList<string> answers, EnumExamPracticeScoreCriteria criteria)
        {
            StringBuilder sb = new StringBuilder();
            string defaultConfigByCriteria = GetConfigByType(criteria, false);

            // Generate questions
            sb.AppendLine("Speaking Test Questions:");
            for (int i = 0; i < questions.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"question{0}\": \"{1}\"\n", NumberToWords(i + 1), questions[i]);
            }

            sb.AppendLine();

            // Generate answers
            sb.AppendLine("Student Submission:");
            for (int i = 0; i < answers.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"answer{0}\": \"{1}\"\n", NumberToWords(i + 1), answers[i]);
            }
            sb.AppendLine();

            string result = sb.ToString();

            result = string.Concat(result, " ", defaultConfigByCriteria);
            return result;
        }

        /// <summary>
        /// config answer , question. để tạo thành prompt gửi cho chatgpt
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private static string CustomAnswerConfigToSendGPT(IList<string> questions, IList<string> answers, ExamPracticeAICriteriaSetting examPracticeAICriteriaSetting)
        {
            StringBuilder sb = new StringBuilder();
            string defaultConfigByCriteria = examPracticeAICriteriaSetting.Prompts?[0]?.PromptContent ?? string.Empty;

            // Generate questions
            sb.AppendLine("Speaking Test Questions:");
            for (int i = 0; i < questions.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"question{0}\": \"{1}\"\n", NumberToWords(i + 1), questions[i]);
            }

            sb.AppendLine();

            // Generate answers
            sb.AppendLine("Student Submission:");
            for (int i = 0; i < answers.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"answer{0}\": \"{1}\"\n", NumberToWords(i + 1), answers[i]);
            }
            sb.AppendLine();

            string result = sb.ToString();

            result = string.Concat(result, " ", defaultConfigByCriteria);

            return result;
        }

        /// <summary>
        /// Lấy config của AI Speaking theo tiêu chí
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="isUserConfig"></param>
        /// <returns></returns>
        private static string GetConfigByType(EnumExamPracticeScoreCriteria criteria, bool isUserConfig)
        {
            string result = string.Empty;
            switch (criteria)
            {
                case EnumExamPracticeScoreCriteria.GrammaticalRangeAndAccuracy:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingGrammarRole) : File.ReadAllText(ResourceSettings.SpeakingGrammar);
                    break;

                case EnumExamPracticeScoreCriteria.LexicalResource:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingLexicalRole) : File.ReadAllText(ResourceSettings.SpeakingLexical);
                    break;

                case EnumExamPracticeScoreCriteria.FluencyAndCoherence:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingFluencyRole) : File.ReadAllText(ResourceSettings.SpeakingFluency);

                    break;
            }

            return result;
        }

        /// <summary>
        /// Chuyển đổi số thành chữ
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string NumberToWords(int number)
        {
            if (number == 0)
            {
                return "Zero";
            }
            if (number < 0)
            {
                return "Minus" + NumberToWords(Math.Abs(number));
            }

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (string.IsNullOrEmpty(words))
                {
                    words += "";
                }
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                {
                    words += unitsMap[number];
                }
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += "-" + unitsMap[number % 10];
                    }
                }
            }

            return words;
        }

        #endregion Func
    }
}
