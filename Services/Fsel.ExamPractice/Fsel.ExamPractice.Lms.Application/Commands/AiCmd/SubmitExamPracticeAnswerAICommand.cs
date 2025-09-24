// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Globalization;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using static Fsel.Shared.Constants.ValueSettings;

    public class SubmitExamPracticeAnswerAICommand : ExamPracticeAnswerResponseModel, IRequest<bool>
    {
    }

    public class SubmitExamPracticeAnswerAICommandHandler : IRequestHandler<SubmitExamPracticeAnswerAICommand, bool>
    {
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private readonly SubmitExamPracticeCriteriaPublisher _submitExamPracticeCriteria;
        private readonly IUserService _userService;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeAISettingRepository _aiGradeSettingRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly SetTimeRetryExamPracticePublisher _setTimeRetryExamPracticePublisher;
        private readonly IMediator _mediator;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly ExamPracticesDBContext _examPracticesDBContext;
        private readonly IMapper _mapper;
        private readonly ILogger<SubmitExamPracticeAnswerAICommandHandler> _logger;
        private const int CorrectTotal_IELTS_Writing = 36;
        private const int CorrectTotal_Vstep_Writing = 40;
        private const int MaxSection = 2;
        private const int Last_DisplayOrder = 2;
        private const int Max_Times_Retry = 3;

        public SubmitExamPracticeAnswerAICommandHandler(IExamPracticeAnswerRepository examPracticeAnswerRepository,
            SubmitExamPracticeCriteriaPublisher submitExamPracticeCriteria,
            IUserService userService,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeAISettingRepository aiGradeSettingRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            SetTimeRetryExamPracticePublisher setTimeRetryExamPracticePublisher,
            IMediator mediator,
            IProsodyScoreRepository prosodyScoreRepository,
            ExamPracticesDBContext examPracticesDBContext,
            IMapper mapper,
            ILogger<SubmitExamPracticeAnswerAICommandHandler> logger)
        {
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
            _submitExamPracticeCriteria = submitExamPracticeCriteria;
            _userService = userService;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
            _setTimeRetryExamPracticePublisher = setTimeRetryExamPracticePublisher;
            _mediator = mediator;
            _prosodyScoreRepository = prosodyScoreRepository;
            _examPracticesDBContext = examPracticesDBContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> Handle(SubmitExamPracticeAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            _logger.LoggerRequest(request);

            var examPracticeSectionResult = await _examPracticeSectionResultRepository.GetByIdAsync(request.ExamPracticeSectionResultId);
            if (examPracticeSectionResult == null)
            {
                return true;
            }

            var examPracticeAnswer = await GetExamPracticeAnswer(request, cancellationToken);
            var aiConfig = await GetAIConfig(request, cancellationToken);
            var examPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(request.ExamPracticeSectionId);
            if (examPracticeSection == null || !IsValidAIConfig(aiConfig))
            {
                return true;
            }
            aiConfig ??= new ExamPracticeAISetting();

            var examPracticeResult = await GetExamPracticeResult(examPracticeSectionResult, cancellationToken);
            if (examPracticeResult == null || examPracticeResult.ExamPractice == null)
            {
                return true;
            }
            var student = await GetStudent(examPracticeResult.StudentId);
            if (student == null)
            {
                return true;
            }
            double averageScore = 0;
            double totalScore = 0;
            bool checkSkillMockTest = false;
            string gradingAiFeedBack = string.Empty;

            var resultDictionary = await GetAIResponses(aiConfig, request, examPracticeSection, examPracticeSectionResult, cancellationToken);

            switch (examPracticeResult.ExamPractice.Type)
            {
                case EnumExamPracticeType.IELTS:
                    (averageScore, totalScore, checkSkillMockTest, gradingAiFeedBack) = await HandleIELTSAsync(examPracticeAnswer, request, resultDictionary, examPracticeResult, cancellationToken);
                    break;

                case EnumExamPracticeType.Vstep:
                    (averageScore, totalScore, checkSkillMockTest, gradingAiFeedBack) = await HandleVstepAsync(examPracticeAnswer, request, resultDictionary, examPracticeResult, cancellationToken);
                    break;
            }
            await UpdateExamPracticeSectionResultAsync(examPracticeSectionResult, examPracticeSection.Id, averageScore, totalScore);

            var skillScores = await UpdateSkillScores(examPracticeSectionResult, examPracticeSection, averageScore, totalScore, checkSkillMockTest, examPracticeResult);
            examPracticeSectionResult.SkillScores = skillScores;

            await UpdateEntities(examPracticeAnswer, gradingAiFeedBack, examPracticeSectionResult, examPracticeResult, checkSkillMockTest, cancellationToken);
            return true;
        }

        private async Task<(double averageScore, double totalScore, bool checkSkillMockTest, string gradingAiFeedBack)> HandleIELTSAsync(
            ExamPracticeAnswer? examPracticeAnswer,
            SubmitExamPracticeAnswerAICommand request,
            Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>> resultDictionary,
            ExamPracticeResult examPracticeResult,
            CancellationToken cancellationToken)
        {
            static double ConverBandScore(IList<ExamPracticeAIGradingLanguageModel>? examPracticeAIGradingLanguages) => double.TryParse(examPracticeAIGradingLanguages?[0]?.ExamPracticeAIGradings?[0]?.BandScore, out double bandScore) ? bandScore : ValueDefault;

            var gradingAiFeedBackResult = new List<AiFeedbackItemModel>
            {
                new AiFeedbackItemModel
                {
                    Criteria = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse)?  EnumMockTestAIType.TaskResponse.ToString() :EnumMockTestAIType.TaskAchievement.ToString(),
                    BandScore = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse) ? ConverBandScore(resultDictionary[EnumMockTestAIType.TaskResponse]) : ConverBandScore(resultDictionary[EnumMockTestAIType.TaskAchievement]),
                    GradingAlFeedbacks = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse) ? resultDictionary[EnumMockTestAIType.TaskResponse] : resultDictionary[EnumMockTestAIType.TaskAchievement]
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.Coherence.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.Coherence)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Coherence)
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.LexicalResource.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.LexicalResource)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.LexicalResource)
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.GrammaticalRange.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.GrammaticalRange)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.GrammaticalRange)
                }
            };
            string? gradingAiFeedBack = gradingAiFeedBackResult.Serialize();

            var taskResponse = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult[0].GradingAlFeedbacks);
            var coherence = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult[1].GradingAlFeedbacks);
            var lexicalResource = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult[2].GradingAlFeedbacks);
            var grammaticalRange = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult[3].GradingAlFeedbacks);

            await HandleRetryIfNeeded(examPracticeAnswer, request, taskResponse, coherence, lexicalResource, grammaticalRange, cancellationToken);
            bool checkSkillMockTest = examPracticeResult.ExamPractice != null && examPracticeResult.ExamPractice.SubType == EnumExamPracticeSubType.SkillMockTest;
            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse, coherence, lexicalResource, grammaticalRange);
            return (averageScore, totalScore, checkSkillMockTest, gradingAiFeedBack);
        }

        private async Task<(double averageScore, double totalScore, bool checkSkillMockTest, string gradingAiFeedBack)> HandleVstepAsync(
            ExamPracticeAnswer? examPracticeAnswer,
            SubmitExamPracticeAnswerAICommand request,
            Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>> resultDictionary,
            ExamPracticeResult examPracticeResult,
            CancellationToken cancellationToken)
        {
            double ConverBandScore(IList<ExamPracticeAIGradingLanguageModel>? examPracticeAIGradingLanguages) => double.TryParse(examPracticeAIGradingLanguages?[0]?.ExamPracticeAIGradings?[0]?.BandScore, out double bandScore) ? bandScore : ValueDefault;
            List<ExamPracticeAIGradingModel>? GetAIGradings(List<ExamPracticeAIGradingLanguageModel>? list) => list != null && list.Count > 0 ? list[0].ExamPracticeAIGradings?.ToList() : null;
            var gradingAiFeedBackResult = new List<AiFeedbackItemModel>
            {
                new AiFeedbackItemModel
                {
                    Criteria =  EnumMockTestAIType.TaskFulfillment.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.TaskFulfillment)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.TaskFulfillment)
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.Organization.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.Organization)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Organization)
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.Vocabulary.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.Vocabulary)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Vocabulary)
                },
                new AiFeedbackItemModel
                {
                    Criteria = EnumMockTestAIType.Grammar.ToString(),
                    BandScore = ConverBandScore(resultDictionary.GetValueOrDefault(EnumMockTestAIType.Grammar)),
                    GradingAlFeedbacks = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Grammar)
                }
            };
            string? gradingAiFeedBack = gradingAiFeedBackResult.Serialize();

            var taskResponseGradings = GetAIGradings(gradingAiFeedBackResult[0].GradingAlFeedbacks?.ToList());
            var organizationGradings = GetAIGradings(gradingAiFeedBackResult[1].GradingAlFeedbacks?.ToList());
            var vocabularyGradings = GetAIGradings(gradingAiFeedBackResult[2].GradingAlFeedbacks?.ToList());
            var grammarGradings = GetAIGradings(gradingAiFeedBackResult[3].GradingAlFeedbacks?.ToList());

            await HandleRetryIfNeeded(examPracticeAnswer, request, taskResponseGradings, organizationGradings, vocabularyGradings, grammarGradings, cancellationToken);

            bool checkSingleVstepSkill = examPracticeResult.ExamPractice?.SubType == EnumExamPracticeSubType.SingleVstepSkill;
            (double averageScore, double totalScore) = CalculateOverallAverageVstep(
                taskResponseGradings,
                organizationGradings,
                vocabularyGradings,
                grammarGradings);

            return (averageScore, totalScore, checkSingleVstepSkill, gradingAiFeedBack);
        }

        private async Task UpdateExamPracticeSectionResultAsync(ExamPracticeSectionResult examPracticeSectionResult, Guid examPracticeSectionId, double score, double totalScore)
        {
            var sectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeSectionResult.ExamPracticeResultId)
                                                                          .Where(x => x.ExamPracticeSectionId == examPracticeSectionId)
                                                                          .FirstOrDefaultAsync();
            if (sectionResult == null)
            {
                return;
            }
            if (sectionResult.SkillScores == null || sectionResult.SkillScores.Count == 0)
            {
                sectionResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        CorrectCount = totalScore,
                        CountQuestion = MaxSection,
                        TotalQuestion = MaxSection,
                        Scores = score,
                        Skill = EnumCourseSkill.Writing,
                        TotalCount = CorrectTotal_Vstep_Writing,
                    }
                };
            }
            else
            {
                sectionResult.SkillScores.Single().Scores = score;
                sectionResult.SkillScores.Single().CorrectCount = totalScore;
            }
            sectionResult.CorrectCount = (int)totalScore;
            sectionResult.CorrectTotal = CorrectTotal_Vstep_Writing;
            sectionResult.Status = EnumResultStatus.Done;
            await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { sectionResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.SkillScoresStr, entity.CorrectTotal, entity.CorrectCount, entity.Status };
            });
        }

        private async Task<ExamPracticeAnswer?> GetExamPracticeAnswer(SubmitExamPracticeAnswerAICommand request, CancellationToken cancellationToken)
        {
            return await _examPracticeAnswerRepository.Queryable
                .Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeSectionResultId == request.ExamPracticeSectionResultId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<ExamPracticeAISetting?> GetAIConfig(SubmitExamPracticeAnswerAICommand request, CancellationToken cancellationToken)
        {
            return await _aiGradeSettingRepository.Queryable.AsNoTracking()
                .Include(x => x.ExamPracticeAICriteriaSettings)
                .FirstOrDefaultAsync(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId, cancellationToken);
        }

        private static bool IsValidAIConfig(ExamPracticeAISetting? aiConfig)
        {
            if (aiConfig == null)
            {
                return false;
            }
            if ((aiConfig.ExamPracticeAICriteriaSettings == null && aiConfig.SystemRoleAlConfig == null) ||
                (aiConfig.ExamPracticeAICriteriaSettings?.Count == 0 && aiConfig.SystemRoleAlConfig == null))
            {
                return false;
            }
            return true;
        }

        private async Task<Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>>> GetAIResponses(
            ExamPracticeAISetting aiConfig,
            SubmitExamPracticeAnswerAICommand request,
            ExamPracticeSection examPracticeSection,
            ExamPracticeSectionResult examPracticeSectionResult,
            CancellationToken cancellationToken)
        {
            var resultDictionary = new Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>>();

            if (IsCriteriaSettingsMode(aiConfig))
            {
                await HandleCriteriaSettingsMode(aiConfig, request, examPracticeSection, examPracticeSectionResult, resultDictionary, cancellationToken);
            }
            else
            {
                await HandlePromptsMode(aiConfig, request, examPracticeSection, examPracticeSectionResult, resultDictionary, cancellationToken);
            }

            return resultDictionary;
        }

        private static bool IsCriteriaSettingsMode(ExamPracticeAISetting aiConfig)
        {
            return string.IsNullOrEmpty(aiConfig.SystemRoleAlConfig) || (aiConfig.Prompts != null && aiConfig.Prompts.Count == 0);
        }

        private async Task HandleCriteriaSettingsMode(
            ExamPracticeAISetting aiConfig,
            SubmitExamPracticeAnswerAICommand request,
            ExamPracticeSection examPracticeSection,
            ExamPracticeSectionResult examPracticeSectionResult,
            Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>> resultDictionary,
            CancellationToken cancellationToken)
        {
            var moduleAIType = examPracticeSection.DisplayOrder == 0 ? EnumExamPracticeModuleAIType.WritingTask1 : EnumExamPracticeModuleAIType.WritingTask2;
            var prosodyScores = await _prosodyScoreRepository.Queryable
                .Where(x => x.ModuleAIType == moduleAIType)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in aiConfig.ExamPracticeAICriteriaSettings)
            {
                if (item.Prompts == null || item.Prompts.Count == 0)
                {
                    continue;
                }
                var userAiConfig = BuildUserAiConfig(aiConfig.Task, item.Prompts.First().PromptContent ?? string.Empty, request.WordContent ?? string.Empty);
                if (userAiConfig == null)
                {
                    continue;
                }
                string aIResponse;
                if (!string.IsNullOrEmpty(item.JsonSchema))
                {
                    aIResponse = await BuildSchemaAIResponseAsync(aiConfig, item, userAiConfig, prosodyScores, cancellationToken);
                }
                else
                {
                    aIResponse = await SendChatGPT(aiConfig, item.SystemRoleAlConfig ?? string.Empty, userAiConfig, cancellationToken);
                }

                resultDictionary[item.Prompts[0].Type] = ConvertHelper.Deserialize<List<ExamPracticeAIGradingLanguageModel>>(aIResponse) ?? new List<ExamPracticeAIGradingLanguageModel>();
                await SendWebSocket(aIResponse, item.Prompts![0].Type.ToString(), examPracticeSection.DisplayOrder, examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
            }
        }

        private async Task<string> BuildSchemaAIResponseAsync(
            ExamPracticeAISetting aiSetting,
            ExamPracticeAICriteriaSetting criteriaSetting,
            string userPrompt,
            List<ProsodyScore> prosodyScores,
            CancellationToken cancellationToken)
        {
            var aiResponse = await SendChatGPTSchema(
                aiSetting,
                criteriaSetting.SystemRoleAlConfig ?? string.Empty,
                userPrompt,
                criteriaSetting.JsonSchema ?? string.Empty,
                cancellationToken);

            var relevantProsodyScores = prosodyScores
                .Where(x => x.AIType == criteriaSetting.CriteriaName)
                .ToList();

            var gradingDataModel = ConvertHelper.Deserialize<ExamPracticeAIGradingDataModel>(aiResponse) ?? new ExamPracticeAIGradingDataModel();
            var gradingModel = _mapper.Map<ExamPracticeAIGradingModel>(gradingDataModel);

            if (double.TryParse(gradingModel.BandScore, out double bandScore))
            {
                var matchedScores = relevantProsodyScores
                    .Where(x => x.MinScore <= bandScore && x.MaxScore >= bandScore)
                    .OrderBy(x => x.Language)
                    .ToList();

                if (matchedScores.Count > 0)
                {
                    var mainComment = matchedScores[0].BandComment ?? string.Empty;
                    var altComment = matchedScores.Count > 1 ? matchedScores[1].BandComment ?? string.Empty : mainComment;

                    gradingModel.BandDescriptorText = mainComment;
                    var aIResponseTranslate = await GetTranslateAIResponse(aiResponse, cancellationToken);
                    var responseTranslateDataModel = ConvertHelper.Deserialize<ExamPracticeAIGradingDataModel>(aIResponseTranslate);
                    var responseTranslateModel = _mapper.Map<ExamPracticeAIGradingModel>(responseTranslateDataModel);
                    var altData = new List<ExamPracticeAIGradingModel>
                    {
                        new ExamPracticeAIGradingModel
                        {
                            BandScore = gradingModel.BandScore,
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
                            ExamPracticeAIGradings = x.Language == LanguageAIModule.English ? new List<ExamPracticeAIGradingModel>
                            {
                                gradingModel
                            } : altData,
                        })
                        .ToList();

                    if (languageModels.Any())
                    {
                        aiResponse = languageModels.Serialize();
                    }
                }
            }
            return aiResponse;
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

        private async Task HandlePromptsMode(
            ExamPracticeAISetting aiConfig,
            SubmitExamPracticeAnswerAICommand request,
            ExamPracticeSection examPracticeSection,
            ExamPracticeSectionResult examPracticeSectionResult,
            Dictionary<EnumMockTestAIType, IList<ExamPracticeAIGradingLanguageModel>> resultDictionary,
            CancellationToken cancellationToken)
        {
            if (aiConfig.Prompts == null || aiConfig.Prompts.Count == 0)
            {
                return;
            }
            foreach (var item in aiConfig.Prompts)
            {
                var userAiConfig = BuildUserAiConfig(aiConfig.Task, item.PromptContent ?? string.Empty, request.WordContent ?? string.Empty);
                if (userAiConfig == null)
                {
                    continue;
                }
                var aIResponse = await SendChatGPT(aiConfig, aiConfig.SystemRoleAlConfig ?? string.Empty, userAiConfig, cancellationToken) ?? string.Empty;
                resultDictionary[item.Type] = ConvertHelper.Deserialize<List<ExamPracticeAIGradingLanguageModel>>(aIResponse) ?? new List<ExamPracticeAIGradingLanguageModel>();
                await SendWebSocket(aIResponse, item.Type.ToString(), examPracticeSection.DisplayOrder, examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
            }
        }

        private static string? BuildUserAiConfig(string? task, string promptContent, string wordContent)
        {
            return string.Format(CultureInfo.InvariantCulture, promptContent, new[] { task, wordContent });
        }

        private async Task<ExamPracticeResult?> GetExamPracticeResult(ExamPracticeSectionResult examPracticeSectionResult, CancellationToken cancellationToken)
        {
            return await _examPracticeResultRepository.Queryable
                                .Include(x => x.ExamPractice)
                                .FirstOrDefaultAsync(x => x.Id == examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
        }

        private async Task<object?> GetStudent(Guid studentId)
        {
            var studentResults = await _userService.GetUserByStudentIdWithCache(studentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                return null;
            }
            return studentResults.Content?.Result;
        }

        private async Task HandleRetryIfNeeded(
            ExamPracticeAnswer? examPracticeAnswer,
            SubmitExamPracticeAnswerAICommand request,
            List<ExamPracticeAIGradingModel>? taskResponse,
            List<ExamPracticeAIGradingModel>? coherence,
            List<ExamPracticeAIGradingModel>? lexicalResource,
            List<ExamPracticeAIGradingModel>? grammaticalRange,
            CancellationToken cancellationToken)
        {
            if (examPracticeAnswer != null && (taskResponse == null || coherence == null || lexicalResource == null || grammaticalRange == null) &&
                examPracticeAnswer.RetryTime <= Max_Times_Retry)
            {
                var model = _mapper.Map<SetTimeRetryExamPracticeModel>(request);
                model.StartDate = DateTime.UtcNow;
                await _setTimeRetryExamPracticePublisher.Publish(model, cancellationToken);
                examPracticeAnswer.RetryTime += 1;
            }
        }

        private async Task<List<SkillScores>> UpdateSkillScores(
            ExamPracticeSectionResult examPracticeSectionResult,
            ExamPracticeSection examPracticeSection,
            double averageScore,
            double totalScore,
            bool checkSkillMockTest,
            ExamPracticeResult examPracticeResult)
        {
            await _examPracticesDBContext.Entry(examPracticeSectionResult).ReloadAsync();
            await _examPracticesDBContext.Entry(examPracticeResult).ReloadAsync();

            var skillScore = examPracticeSectionResult.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);
            var skillScores = examPracticeSectionResult.SkillScores?.ToList() ?? new List<SkillScores>();
            var correctTotal = examPracticeResult.ExamPractice?.Type == EnumExamPracticeType.IELTS ? CorrectTotal_IELTS_Writing : CorrectTotal_Vstep_Writing;

            if (skillScore?.TotalCount == ValueDefault)
            {
                skillScores.Single().CorrectCount = totalScore;
                skillScores.Single().Skill = EnumCourseSkill.Writing;
                skillScores.Single().Scores = averageScore;
                skillScores.Single().TotalQuestion = MaxSection;
                skillScores.Single().CountQuestion = MaxSection;
                skillScores.Single().TotalCount = correctTotal;
            }
            else
            {
                skillScore = skillScores.Single();
                int correcCount = (int)CaculateAverageScoreWritingSection(skillScore!.CorrectCount, totalScore, examPracticeSection.DisplayOrder);
                averageScore = CaculateAverageScoreWritingSection(skillScore!.Scores, averageScore, examPracticeSection.DisplayOrder);
                skillScore.CorrectCount = correcCount;
                skillScore.Scores = averageScore;
                examPracticeSectionResult.CorrectCount = correcCount;

                var skillScoreResults = examPracticeResult.SkillScores ?? new List<SkillScores>();
                var skillScoreResult = skillScoreResults.Single(x => x.Skill == EnumCourseSkill.Writing);

                if (checkSkillMockTest)
                {
                    examPracticeResult.CorrectCount = correcCount;
                    examPracticeResult.SkillScores = skillScores;
                    examPracticeResult.CorrectTotal = correctTotal;
                }
                else if (skillScoreResult.TotalCount == ValueDefault)
                {
                    skillScoreResult.TotalCount = correctTotal;
                    skillScoreResult.CorrectCount = correcCount;
                    skillScoreResult.Scores = averageScore;

                    examPracticeResult.CorrectCount += correcCount;
                    examPracticeResult.SkillScores = skillScoreResults;
                }
            }

            return skillScores;
        }

        private async Task UpdateEntities(
            ExamPracticeAnswer? examPracticeAnswer,
            string? gradingAiFeedBack,
            ExamPracticeSectionResult examPracticeSectionResult,
            ExamPracticeResult examPracticeResult,
            bool checkSkillMockTest,
            CancellationToken cancellationToken)
        {
            if (examPracticeAnswer != null)
            {
                examPracticeAnswer.GradingAlFeedback = gradingAiFeedBack;
                _examPracticeAnswerRepository.Update(examPracticeAnswer);
                await _examPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.SkillScoresStr };
                });

                if (checkSkillMockTest)
                {
                    await _examPracticeResultRepository.BulkUpdateList(new List<ExamPracticeResult> { examPracticeResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.SkillScoresStr };
                    });
                }
            }
        }

        private static double CaculateAverageScore(List<ExamPracticeAIGradingModel>? bandScoreDescription)
        {
            double bandScore = 0;

            if (bandScoreDescription == null || bandScoreDescription.Count == 0)
            {
                return bandScore;
            }

            var firstItem = bandScoreDescription.FirstOrDefault()!;

            if (firstItem == null || firstItem.BandScore == null || string.IsNullOrEmpty(firstItem.BandScore))
            {
                return bandScore;
            }

            if (!double.TryParse(firstItem.BandScore, out bandScore))
            {
                bandScore = 0;
            }
            return bandScore;
        }

        private static (double average, double totalScore) CalculateOverallAverage(params List<ExamPracticeAIGradingModel>?[] bandScoreDescriptions)
        {
            double totalScore = 0;

            foreach (var bandScoreDescription in bandScoreDescriptions)
            {
                totalScore += CaculateAverageScore(bandScoreDescription);
            }

            double average = totalScore / bandScoreDescriptions.Length;
            return (NumberHelper.RoundReduceNumber(average), totalScore);
        }

        private static (double average, double totalScore) CalculateOverallAverageVstep(params List<ExamPracticeAIGradingModel>?[] bandScoreDescriptions)
        {
            double totalScore = 0;

            foreach (var bandScoreDescription in bandScoreDescriptions)
            {
                totalScore += CaculateAverageScore(bandScoreDescription);
            }

            double average = totalScore / bandScoreDescriptions.Length;
            return (NumberHelper.RoundNumberDouble(average), totalScore);
        }

        private static double CaculateAverageScoreWritingSection(double firstScore, double average, int displayOrder)
        {
            if (displayOrder == Last_DisplayOrder)
            {
                return NumberHelper.RoundNumberDouble((firstScore + average * 2) / 3);
            }

            return NumberHelper.RoundNumberDouble((average + firstScore * 2) / 3);
        }

        private async Task<string> SendChatGPT(ExamPracticeAISetting aiConfig, string systemRole, string userAiConfig, CancellationToken cancellationToken)
        {
            string aIResponse = "";
            if (string.IsNullOrEmpty(userAiConfig))
            {
                return aIResponse;
            }

            var result = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = aiConfig.SettingModel,
                SettingTemperature = aiConfig.SettingTemperature,
                SettingFrequecy = aiConfig.SettingFrequecy,
                SettingWordMaxLength = aiConfig.SettingWordMaxLength,
                SettingPresence = aiConfig.SettingPresence,
                SettingTopP = aiConfig.SettingTopP,
                SystemRoleAlConfig = systemRole,
                UserAIConfig = userAiConfig
            }, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result))
            {
                aIResponse = result;
            }
            return aIResponse;
        }

        private async Task<string> SendChatGPTSchema(ExamPracticeAISetting aiConfig, string systemRole, string userAiConfig, string jsonSchema, CancellationToken cancellationToken)
        {
            string aIResponse = "";
            if (string.IsNullOrEmpty(userAiConfig))
            {
                return aIResponse;
            }

            var result = await _mediator.Send(new V1i1.SubmitAICommand
            {
                SettingModel = ChatBotSetup.O4MINI,
                SettingTemperature = aiConfig.SettingTemperature,
                SettingFrequecy = aiConfig.SettingFrequecy,
                SettingWordMaxLength = aiConfig.SettingWordMaxLength,
                SettingPresence = aiConfig.SettingPresence,
                SettingTopP = aiConfig.SettingTopP,
                SystemRoleAlConfig = systemRole,
                UserAIConfig = userAiConfig,
                Format = jsonSchema
            }, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result))
            {
                aIResponse = result;
            }
            return aIResponse;
        }

        private async Task SendWebSocket(string aIResponse, string type, int displayOrder, Guid examPracticeResultId, CancellationToken cancellationToken)
        {
            await _submitExamPracticeCriteria.Publish(new SubmitExamPracticeResponseModel
            {
                GradingAlFeedBack = aIResponse,
                CriteriaName = type,
                DisplayOrder = displayOrder,
                ExamPracticeResultId = examPracticeResultId
            }, cancellationToken);
        }
    }
}
