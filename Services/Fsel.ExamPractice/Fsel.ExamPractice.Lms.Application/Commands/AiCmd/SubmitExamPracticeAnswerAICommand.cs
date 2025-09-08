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
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
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
        private readonly IMapper _mapper;
        private const int CorrectTotal_IELTS_Writing = 36;
        private const int CorrectTotal_Vstep_Writing = 40;
        private const int MaxSection = 2;
        private const int Last_DisplayOrder = 1;
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
            IMapper mapper)
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
            _mapper = mapper;
        }

        public async Task<bool> Handle(SubmitExamPracticeAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

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

            var skillScores = UpdateSkillScores(examPracticeSectionResult, examPracticeSection, averageScore, totalScore, checkSkillMockTest, examPracticeResult);
            examPracticeSectionResult.SkillScores = skillScores;

            await UpdateEntities(examPracticeAnswer, gradingAiFeedBack, examPracticeSectionResult, examPracticeResult, checkSkillMockTest, cancellationToken);
            return true;
        }

        private async Task<(double averageScore, double totalScore, bool checkSkillMockTest, string gradingAiFeedBack)> HandleIELTSAsync(
            ExamPracticeAnswer? examPracticeAnswer,
            SubmitExamPracticeAnswerAICommand request,
            Dictionary<EnumMockTestAIType, string> resultDictionary,
            ExamPracticeResult examPracticeResult,
            CancellationToken cancellationToken)
        {
            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse) ? resultDictionary[EnumMockTestAIType.TaskResponse] : resultDictionary[EnumMockTestAIType.TaskAchievement],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };
            string? gradingAiFeedBack = gradingAiFeedBackResult.Serialize();

            var taskResponse = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult.TaskResponse);
            var coherence = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult.Coherence);
            var lexicalResource = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(gradingAiFeedBackResult.GrammaticalRange);

            await HandleRetryIfNeeded(examPracticeAnswer, request, taskResponse, coherence, lexicalResource, grammaticalRange, cancellationToken);
            bool checkSkillMockTest = examPracticeResult.ExamPractice != null && examPracticeResult.ExamPractice.SubType == EnumExamPracticeSubType.SkillMockTest;
            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse, coherence, lexicalResource, grammaticalRange);
            return (averageScore, totalScore, checkSkillMockTest, gradingAiFeedBack);
        }

        private async Task<(double averageScore, double totalScore, bool checkSkillMockTest, string gradingAiFeedBack)> HandleVstepAsync(
            ExamPracticeAnswer? examPracticeAnswer,
            SubmitExamPracticeAnswerAICommand request,
            Dictionary<EnumMockTestAIType, string> resultDictionary,
            ExamPracticeResult examPracticeResult,
            CancellationToken cancellationToken)
        {
            string GetResult(EnumMockTestAIType type, EnumMockTestAIType fallback) => resultDictionary.TryGetValue(type, out var value) ? value
                                                                                    : resultDictionary.GetValueOrDefault(fallback) ?? string.Empty;
            List<ExamPracticeAIGradingLanguageModel>? GetGrading(string? json) => ConvertHelper.Deserialize<List<ExamPracticeAIGradingLanguageModel>>(json);
            List<ExamPracticeAIGradingModel>? GetAIGradings(List<ExamPracticeAIGradingLanguageModel>? list) => list != null && list.Count > 0 ? list[0].ExamPracticeAIGradings?.ToList() : null;

            var gradingAiFeedBackResult = new
            {
                TaskResponse = GetResult(EnumMockTestAIType.TaskResponse, EnumMockTestAIType.TaskAchievement),
                Coherence = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Coherence),
                LexicalResource = resultDictionary.GetValueOrDefault(EnumMockTestAIType.LexicalResource),
                GrammaticalRange = resultDictionary.GetValueOrDefault(EnumMockTestAIType.GrammaticalRange)
            };
            string gradingAiFeedBack = gradingAiFeedBackResult.Serialize();
            var taskResponse = GetGrading(gradingAiFeedBackResult.TaskResponse);
            var coherence = GetGrading(gradingAiFeedBackResult.Coherence);
            var lexicalResource = GetGrading(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = GetGrading(gradingAiFeedBackResult.GrammaticalRange);

            var taskResponseGradings = GetAIGradings(taskResponse);
            var coherenceGradings = GetAIGradings(coherence);
            var lexicalResourceGradings = GetAIGradings(lexicalResource);
            var grammaticalRangeGradings = GetAIGradings(grammaticalRange);

            await HandleRetryIfNeeded(examPracticeAnswer, request, taskResponseGradings, coherenceGradings, lexicalResourceGradings, grammaticalRangeGradings, cancellationToken);

            bool checkSingleVstepSkill = examPracticeResult.ExamPractice?.SubType == EnumExamPracticeSubType.SingleVstepSkill;
            (double averageScore, double totalScore) = CalculateOverallAverage(
                taskResponseGradings,
                coherenceGradings,
                lexicalResourceGradings,
                grammaticalRangeGradings);

            return (averageScore, totalScore, checkSingleVstepSkill, gradingAiFeedBack);
        }

        private async Task UpdateExamPracticeSectionResultAsync(ExamPracticeSectionResult examPracticeSectionResult, Guid examPracticeSectionId, double score, double totalScore)
        {
            var sectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeSectionResult.ExamPracticeResultId)
                                                                          .Where(x => x.ExamPracticeSectionId == examPracticeSectionId)
                                                                          .FirstOrDefaultAsync();
            if (sectionResult == null || sectionResult.SkillScores == null)
            {
                return;
            }
            sectionResult.SkillScores.Single().Scores = score;
            sectionResult.SkillScores.Single().CorrectCount = totalScore;
            _examPracticeSectionResultRepository.Update(sectionResult, false, x => x.WorkingTime);
            await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
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

        private async Task<Dictionary<EnumMockTestAIType, string>> GetAIResponses(
            ExamPracticeAISetting aiConfig,
            SubmitExamPracticeAnswerAICommand request,
            ExamPracticeSection examPracticeSection,
            ExamPracticeSectionResult examPracticeSectionResult,
            CancellationToken cancellationToken)
        {
            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

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
            Dictionary<EnumMockTestAIType, string> resultDictionary,
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

                resultDictionary[item.Prompts[0].Type] = aIResponse;
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

            var gradingModels = ConvertHelper.Deserialize<List<ExamPracticeAIGradingModel>>(aiResponse) ?? new List<ExamPracticeAIGradingModel>();

            if (gradingModels.Count > 0 && double.TryParse(gradingModels[0].BandScore, out double bandScore))
            {
                var matchedScores = relevantProsodyScores
                    .Where(x => x.MinScore <= bandScore && x.MaxScore >= bandScore)
                    .ToList();

                if (matchedScores.Count > 0)
                {
                    var mainComment = matchedScores[0].BandComment ?? string.Empty;
                    var altComment = matchedScores.Count > 1 ? matchedScores[1].BandComment ?? string.Empty : mainComment;

                    foreach (var model in gradingModels)
                    {
                        model.BandDescriptorText = mainComment;
                    }
                    var aIResponseTranslate = await GetTranslateAIResponse(aiResponse, cancellationToken);
                    var responseTranslateModel = ConvertHelper.Deserialize<ExamPracticeAIGradingModel>(aIResponseTranslate);

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
            Dictionary<EnumMockTestAIType, string> resultDictionary,
            CancellationToken cancellationToken)
        {
            if (aiConfig.Prompts == null || aiConfig.Prompts.Count == 0)
            {
                return;
            }
            foreach (var item in aiConfig.Prompts)
            {
                var userAiConfig = BuildUserAiConfig(aiConfig.Task, item.PromptContent, request.WordContent ?? string.Empty);
                if (userAiConfig == null)
                {
                    continue;
                }
                var aIResponse = await SendChatGPT(aiConfig, aiConfig.SystemRoleAlConfig ?? string.Empty, userAiConfig, cancellationToken) ?? string.Empty;
                resultDictionary[item.Type] = aIResponse;
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

        private static List<SkillScores> UpdateSkillScores(
            ExamPracticeSectionResult examPracticeSectionResult,
            ExamPracticeSection examPracticeSection,
            double averageScore,
            double totalScore,
            bool checkSkillMockTest,
            ExamPracticeResult examPracticeResult)
        {
            var skillScore = examPracticeSectionResult.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);
            var skillScores = examPracticeSectionResult.SkillScores?.ToList() ?? new List<SkillScores>();
            var correctTotal = examPracticeResult.ExamPractice?.Type == EnumExamPracticeType.IELTS ? CorrectTotal_IELTS_Writing : CorrectTotal_Vstep_Writing;

            if (skillScore?.TotalCount == default)
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
                skillScores.Single().CorrectCount = correcCount;
                skillScores.Single().Scores = averageScore;
                examPracticeSectionResult.CorrectCount = correcCount;

                if (checkSkillMockTest)
                {
                    examPracticeResult.CorrectCount = correcCount;
                    examPracticeResult.SkillScores = skillScores;
                    examPracticeResult.CorrectTotal = correctTotal;
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

                _examPracticeSectionResultRepository.Update(examPracticeSectionResult, false, x => x.WorkingTime);
                await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (checkSkillMockTest)
                {
                    _examPracticeResultRepository.Update(examPracticeResult);
                    await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
