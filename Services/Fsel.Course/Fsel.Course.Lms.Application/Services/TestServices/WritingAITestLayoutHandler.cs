// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.AiCmd.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers.Test;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using SharedStringHelper = Shared.Helpers.StringHelper;

    public sealed class WritingAITestLayoutHandler : IWritingAITestLayoutHandler
    {
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly ITestAISettingRepository _aiGradeSettingRepository;
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly IUserService _userService;
        private readonly SubmitTestCriteriaPublisher _submitTestCriteriaPublisher;
        private readonly SetTimeRetryTestPublisher _setTimeRetryTestPublisher;
        private readonly ISenderService _senderService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;

        private const int CorrectTotal_Writing = 36;
        private const int First_Run_Order = 1;
        private const int Max_Times_Retry = 3;

        public WritingAITestLayoutHandler(
            IRepository<TestAnswer> testAnswerRepository,
            ITestAISettingRepository aiGradeSettingRepository,
            ITestSectionResultRepository testSectionResultRepository,
            ITestResultRepository testResultRepository,
            IUserService userService,
            SubmitTestCriteriaPublisher submitTestCriteriaPublisher,
            SetTimeRetryTestPublisher setTimeRetryTestPublisher,
            ISenderService senderService,
            IMediator mediator,
            IMapper mapper,
            AppSetting appSetting)
        {
            _testAnswerRepository = testAnswerRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
            _testSectionResultRepository = testSectionResultRepository;
            _testResultRepository = testResultRepository;
            _userService = userService;
            _submitTestCriteriaPublisher = submitTestCriteriaPublisher;
            _setTimeRetryTestPublisher = setTimeRetryTestPublisher;
            _senderService = senderService;
            _mediator = mediator;
            _mapper = mapper;
            _appSetting = appSetting;
        }

        public async Task HandleAsync(TestSectionResult testSectionResult, TestResult testResult, CancellationToken cancellationToken)
        {
            var testAnswers = await LoadAnswersAsync(testSectionResult.Id, cancellationToken);
            if (testAnswers.Count == 0)
            {
                return;
            }

            if (!testSectionResult.TestSectionId.HasValue)
            {
                return;
            }

            var aiConfig = await LoadAiConfigAsync(testSectionResult.TestSectionId.Value, cancellationToken);
            if (!IsValidAiConfig(aiConfig))
            {
                return;
            }

            if (testSectionResult.SkillScores == null)
            {
                return;
            }

            var student = await LoadStudentAsync(testResult.StudentId);
            var sectionDisplayOrder = testSectionResult.TestSection?.DisplayOrder ?? 0;
            var skillId = testSectionResult.TestSection?.SkillId;

            // SkillScores trên section result
            var sectionSkillScores = EnsureSectionSkillScore(
                current: testSectionResult.SkillScores.ToList(),
                skillId: skillId);

            var sectionSkillScore = sectionSkillScores.First(x => x.SkillId == skillId);

            // Chạy từng Answer 1 (for loop)
            for (int i = 0; i < testAnswers.Count; i++)
            {
                var answer = testAnswers[i];
                int runOrder = i == 0 ? First_Run_Order : (i + 1);

                await EvaluateSingleAnswerAsync(
                    answer: answer,
                    aiConfig: aiConfig!,
                    wordContent: BuildWritingContent(answer),
                    sectionDisplayOrder: sectionDisplayOrder,
                    testResultId: testResult.Id,
                    runOrder: runOrder,
                    sectionSkillScore: sectionSkillScore,
                    studentEmail: student?.User?.Email,
                    testName: testResult.Test?.Name,
                    cancellationToken: cancellationToken);
            }

            // Update lại skill scores + correct count cho section result
            testSectionResult.SkillScores = sectionSkillScores;
            testSectionResult.CorrectCount = (int)sectionSkillScore.CorrectCount;

            // Merge lên TestResult theo nhiều Skill (bỏ check TestType)
            testResult.SkillScores = MergeSkillScores(
                current: testResult.SkillScores?.ToList(),
                updatedSectionScores: sectionSkillScores,
                skillId: skillId);

            // Persist: answers + sectionResult + testResult
            await PersistAnswersAsync(testAnswers, cancellationToken);
            await SaveTestSectionResultAsync(testSectionResult, cancellationToken);
            await PersistTestResultAsync(testResult, cancellationToken);
        }

        private async Task<List<TestAnswer>> LoadAnswersAsync(Guid testSectionResultId, CancellationToken cancellationToken)
        {
            return await _testAnswerRepository.Queryable
                .Include(x => x.TestSection)
                .Where(x => x.TestSectionResultId == testSectionResultId)
                .ToListAsync(cancellationToken);
        }

        private async Task<TestAISetting?> LoadAiConfigAsync(Guid testSectionId, CancellationToken cancellationToken)
        {
            return await _aiGradeSettingRepository.ReadQueryable
                .Include(x => x.TestAICriteriaSettings)
                .FirstOrDefaultAsync(x => x.TestSectionId == testSectionId, cancellationToken);
        }

        private static bool IsValidAiConfig(TestAISetting? aiConfig)
        {
            if (aiConfig == null)
            {
                return false;
            }

            var noCriteriaAndNoSystemRole =
                (aiConfig.TestAICriteriaSettings == null || aiConfig.TestAICriteriaSettings.Count == 0)
                && aiConfig.SystemRoleAlConfig == null;

            return !noCriteriaAndNoSystemRole;
        }

        private async Task<StudentModel?> LoadStudentAsync(Guid studentId)
        {
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                return null;
            }

            return studentResults.Content?.Result?.FirstOrDefault();
        }

        private async Task EvaluateSingleAnswerAsync(
            TestAnswer answer,
            TestAISetting aiConfig,
            string wordContent,
            int sectionDisplayOrder,
            Guid testResultId,
            int runOrder,
            SkillScores sectionSkillScore,
            string? studentEmail,
            string? testName,
            CancellationToken cancellationToken)
        {
            // 1) Call AI -> resultDictionary
            var resultDictionary = await CallAiForAnswerAsync(
                aiConfig: aiConfig,
                wordContent: wordContent,
                sectionDisplayOrder: sectionDisplayOrder,
                testResultId: testResultId,
                cancellationToken: cancellationToken);

            // 2) Build feedback object
            var gradingAiFeedBackResult = BuildFeedbackObject(resultDictionary);

            // 3) Parse bandscore
            var parsed = ParseFeedback(gradingAiFeedBackResult);

            // 4) Retry per answer
            await HandleRetryAsync(
                answer,
                parsed.TaskResponse,
                parsed.Coherence,
                parsed.LexicalResource,
                parsed.GrammaticalRange,
                studentEmail,
                testName,
                cancellationToken);

            // 5) Save feedback vào chính answer
            answer.GradingAlFeedback = ConvertHelper.Serialize(gradingAiFeedBackResult);

            // 6) Nếu thiếu data -> dừng
            if (!parsed.IsValid)
            {
                return;
            }

            // 7) Tính điểm + blend vào skillScore
            (double averageScore, double totalScore) = CalculateOverallAverage(
                parsed.TaskResponse!,
                parsed.Coherence!,
                parsed.LexicalResource!,
                parsed.GrammaticalRange!);

            BlendWritingScore(sectionSkillScore, averageScore, totalScore, runOrder);
        }

        private async Task<Dictionary<EnumMockTestAIType, string>> CallAiForAnswerAsync(
            TestAISetting aiConfig,
            string wordContent,
            int sectionDisplayOrder,
            Guid testResultId,
            CancellationToken cancellationToken)
        {
            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            // Case 1: criteria settings
            if (string.IsNullOrEmpty(aiConfig.SystemRoleAlConfig) || (aiConfig.Prompts != null && aiConfig.Prompts.Count == 0))
            {
                foreach (var item in aiConfig.TestAICriteriaSettings)
                {
                    var prompt = string.Concat(new[] { aiConfig.Task!, Environment.NewLine, item.AIConfigs!.Single().PromptContent! });
                    var userAiConfig = prompt.Replace("{0}", wordContent, StringComparison.CurrentCulture);

                    var aiResponse = await SendChatGPT(aiConfig, item.AIConfigStr!, userAiConfig, cancellationToken);
                    aiResponse = SharedStringHelper.RemoveMarkdownFromJson(aiResponse);

                    var type = item.AIConfigs![0].Type;
                    resultDictionary[type] = aiResponse;

                    await SendWebSocket(aiResponse, type.ToString(), sectionDisplayOrder, testResultId, cancellationToken);
                }

                return resultDictionary;
            }

            // Case 2: common prompts
            foreach (var item in aiConfig.Prompts!)
            {
                var prompt = string.Concat(new[] { aiConfig.Task!, Environment.NewLine, item.PromptContent! });
                var userAiConfig = prompt.Replace("{0}", wordContent, StringComparison.CurrentCulture);

                var aiResponse = await SendChatGPT(aiConfig, aiConfig.SystemRoleAlConfig!, userAiConfig, cancellationToken);
                aiResponse = SharedStringHelper.RemoveMarkdownFromJson(aiResponse);

                resultDictionary[item.Type] = aiResponse;

                await SendWebSocket(aiResponse, item.Type.ToString(), sectionDisplayOrder, testResultId, cancellationToken);
            }

            return resultDictionary;
        }

        private static object BuildFeedbackObject(Dictionary<EnumMockTestAIType, string> resultDictionary)
        {
            return new
            {
                TaskResponse = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse)
                    ? resultDictionary[EnumMockTestAIType.TaskResponse]
                    : resultDictionary.GetValueOrDefault(EnumMockTestAIType.TaskAchievement),
                Coherence = resultDictionary.GetValueOrDefault(EnumMockTestAIType.Coherence),
                LexicalResource = resultDictionary.GetValueOrDefault(EnumMockTestAIType.LexicalResource),
                GrammaticalRange = resultDictionary.GetValueOrDefault(EnumMockTestAIType.GrammaticalRange)
            };
        }

        private static (
            List<TestAIGradingModel>? TaskResponse,
            List<TestAIGradingModel>? Coherence,
            List<TestAIGradingModel>? LexicalResource,
            List<TestAIGradingModel>? GrammaticalRange,
            bool IsValid)
            ParseFeedback(object feedback)
        {
            // feedback là anonymous object -> serialize rồi deserialize theo key
            var json = ConvertHelper.Serialize(feedback);
            var dict = ConvertHelper.Deserialize<Dictionary<string, string>>(json);

            var taskResponse = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("TaskResponse"));
            var coherence = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("Coherence"));
            var lexical = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("LexicalResource"));
            var grammar = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("GrammaticalRange"));

            var ok = taskResponse != null && coherence != null && lexical != null && grammar != null;

            return (taskResponse, coherence, lexical, grammar, ok);
        }

        private static void BlendWritingScore(SkillScores skillScore, double averageScore, double totalScore, int runOrder)
        {
            // init
            if (skillScore.TotalCount == 0)
            {
                skillScore.TotalCount = CorrectTotal_Writing;
            }

            // blend theo công thức cũ, nhưng runOrder theo vòng for
            skillScore.CorrectCount = (int)CaculateAverageScoreWritingSection(skillScore.CorrectCount, totalScore, runOrder);
            skillScore.Scores = CaculateAverageScoreWritingSection(skillScore.Scores, averageScore, runOrder);
        }

        // =========================
        // Retry
        // =========================

        private async Task HandleRetryAsync(
            TestAnswer answer,
            List<TestAIGradingModel>? taskResponse,
            List<TestAIGradingModel>? coherence,
            List<TestAIGradingModel>? lexicalResource,
            List<TestAIGradingModel>? grammaticalRange,
            string? studentEmail,
            string? testName,
            CancellationToken cancellationToken)
        {
            if (answer.RetryTime > Max_Times_Retry)
            {
                var model = new SendEmailCommandModel
                {
                    ToEmails = new List<string> { _appSetting!.CustomerSupportConfig!.Email! },
                    CcEmails = _appSetting!.CustomerSupportConfig!.CCEmail!,
                    Content = string.Format(ValueSettings.CustomerSupport.Content, studentEmail ?? default, testName ?? default),
                    Subject = string.Format(ValueSettings.CustomerSupport.TitleMail, studentEmail ?? default),
                };

                await _senderService.SendEmailAsync(model);
                return;
            }

            if (taskResponse == null || coherence == null || lexicalResource == null || grammaticalRange == null)
            {
                var retryModel = _mapper.Map<SetTimeRetryTestModel>(new { });
                retryModel.StartDate = DateTime.UtcNow;

                await _setTimeRetryTestPublisher.Publish(retryModel, cancellationToken);

                answer.RetryTime += 1;
            }
        }

        private async Task PersistAnswersAsync(List<TestAnswer> answers, CancellationToken cancellationToken)
        {
            await _testAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                _testAnswerRepository.UpdateList(answers);
                await _testAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        private async Task PersistTestResultAsync(TestResult testResult, CancellationToken cancellationToken)
        {
            try
            {
                await _testResultRepository.BulkUpdateList(new List<TestResult> { testResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.TestGroupResultId, c.StudentId };
                });

                await _testResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // log nếu cần
            }
        }

        private static List<SkillScores> EnsureSectionSkillScore(List<SkillScores> current, Guid? skillId)
        {
            if (skillId == null)
            {
                return current;
            }

            var skillScore = current.FirstOrDefault(x => x.SkillId == skillId);
            if (skillScore != null)
            {
                return current;
            }

            current.Add(new SkillScores
            {
                SkillId = skillId,
                CorrectCount = 0,
                Scores = 0,
                TotalCount = CorrectTotal_Writing,
                TotalQuestion = 2,
                CountQuestion = 2,
            });

            return current;
        }

        private static List<SkillScores> MergeSkillScores(
            List<SkillScores>? current,
            List<SkillScores> updatedSectionScores,
            Guid? skillId)
        {
            current ??= new List<SkillScores>();

            if (skillId == null)
            {
                return updatedSectionScores;
            }

            // remove old skill
            var cloned = current.Where(x => x.SkillId != skillId).ToList();

            // add updated skill
            var toAdd = updatedSectionScores.FirstOrDefault(x => x.SkillId == skillId);
            if (toAdd != null)
            {
                cloned.Add(toAdd);
            }

            return cloned;
        }

        private static string BuildWritingContent(TestAnswer answer)
        {
            // TODO: đổi sang field writing thật của bạn nếu không phải SpeechTextAnswer
            return answer.SpeechTextAnswer ?? string.Empty;
        }

        private static double CaculateAverageScore(List<TestAIGradingModel>? bandScoreDescription)
        {
            if (bandScoreDescription == null || bandScoreDescription.Count == 0)
            {
                return 0;
            }

            var firstItem = bandScoreDescription.FirstOrDefault();
            if (string.IsNullOrEmpty(firstItem?.BandScore))
            {
                return 0;
            }

            double.TryParse(firstItem.BandScore, out var bandScore);
            return bandScore;
        }

        private static (double average, double totalScore) CalculateOverallAverage(params List<TestAIGradingModel>[] bandScoreDescriptions)
        {
            double totalScore = 0;
            foreach (var bandScoreDescription in bandScoreDescriptions)
            {
                totalScore += CaculateAverageScore(bandScoreDescription);
            }

            var average = totalScore / bandScoreDescriptions.Length;
            return (NumberHelper.RoundReduceNumber(average), totalScore);
        }

        private static double CaculateAverageScoreWritingSection(double firstScore, double average, int displayOrder)
        {
            if (displayOrder == First_Run_Order)
            {
                return NumberHelper.RoundNumberDouble((firstScore + average * 2) / 3);
            }

            return NumberHelper.RoundNumberDouble((average + firstScore * 2) / 3);
        }

        private async Task<string> SendChatGPT(TestAISetting aiConfig, string systemRole, string userAiConfig, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userAiConfig))
            {
                return string.Empty;
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
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);

            return result ?? string.Empty;
        }

        private async Task SendWebSocket(string aiResponse, string type, int displayOrder, Guid testResultId, CancellationToken cancellationToken)
        {
            await _submitTestCriteriaPublisher.Publish(new SubmitTestResponseModel
            {
                GradingAlFeedBack = aiResponse,
                CriteriaName = type,
                DisplayOrder = displayOrder,
                TestResultId = testResultId
            }, cancellationToken);
        }
    }
}
