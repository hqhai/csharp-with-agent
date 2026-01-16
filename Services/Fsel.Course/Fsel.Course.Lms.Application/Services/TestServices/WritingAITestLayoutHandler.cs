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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
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

        /// <summary>
        /// Refactor: xử lý ALL TestSectionResult của 1 TestResult.
        /// Answers map theo TestSectionId trong Answer, tương ứng TestSectionId trong SectionResult.
        /// Update Result bé (section) + Result lớn (test).
        /// </summary>
        public async Task HandleAsync(TestSectionResult testSectionResult, TestResult testResult, CancellationToken cancellationToken)
        {
            // 1) Load toàn bộ SectionResult thuộc TestResult (ví dụ Writing có 3 section result)
            var sectionResults = await LoadAllSectionResultsAsync(testSectionResult.Id, cancellationToken);
            if (sectionResults.Count == 0)
            {
                return;
            }

            // 2) Map TestSectionId -> SectionResult
            var sectionResultBySectionId = sectionResults
                .Where(x => x.TestSectionId.HasValue)
                .GroupBy(x => x.TestSectionId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            // 3) Load all answers của các SectionResultId
            var sectionResultIds = sectionResults.Select(x => x.Id).ToList();
            var allAnswers = await LoadAnswersBySectionResultIdsAsync(sectionResultIds, cancellationToken);
            if (allAnswers.Count == 0)
            {
                return;
            }

            // 4) Student info
            var student = await LoadStudentAsync(testResult.StudentId);
            var scoringFormulaType = testResult.Test?.ScoringFormulaType;
            // 5) Ensure SkillScores của Result lớn
            testSectionResult.SkillScores = new List<SkillScores>()
            {
                new SkillScores
                {
                    SkillId = testSectionResult.TestSection?.SkillId,
                    SkillName = testSectionResult.TestSection?.Skill?.Name,
                    SkillFilePath = testSectionResult.TestSection?.Skill?.FilePath,
                    TotalQuestion = 2,
                    CountQuestion = 2,
                }
            };

            // global run order theo skill (blend Result lớn theo công thức hiện tại)
            var globalRunOrderBySkill = new Dictionary<Guid, int>();

            // 6) Group answers theo TestSectionId (đúng yêu cầu)
            var answersBySectionId = allAnswers
                .Where(a => a.TestSectionId.HasValue)
                .GroupBy(a => a.TestSectionId!.Value)
                .ToList();

            foreach (var sectionGroup in answersBySectionId)
            {
                var sectionId = sectionGroup.Key;

                if (!sectionResultBySectionId.TryGetValue(sectionId, out var currentSectionResult))
                {
                    continue;
                }

                // guard
                if (!currentSectionResult.TestSectionId.HasValue)
                {
                    continue;
                }
                currentSectionResult.SkillScores ??= new List<SkillScores>()
                {
                    new SkillScores
                    {
                        SkillId = testSectionResult.TestSection?.SkillId,
                        SkillName = testSectionResult.TestSection?.Skill?.Name,
                        SkillFilePath = testSectionResult.TestSection?.Skill?.FilePath,
                        TotalQuestion = 1,
                        CountQuestion = 1,
                    }
                };

                var sectionDisplayOrder = currentSectionResult.TestSection?.DisplayOrder ?? 0;
                var skillId = currentSectionResult.TestSection?.SkillId;
                if (!skillId.HasValue)
                {
                    continue;
                }

                // load AI config theo sectionId
                var aiConfig = await LoadAiConfigAsync(currentSectionResult.TestSectionId.Value, cancellationToken);
                if (!IsValidAiConfig(aiConfig))
                {
                    continue;
                }

                // Result bé: Ensure section skill score
                var sectionSkillScores = EnsureSectionSkillScore(
                    current: currentSectionResult.SkillScores.ToList(),
                    skillId: skillId);

                var sectionSkillScore = sectionSkillScores.First(x => x.SkillId == skillId);

                // Result lớn: Ensure test skill score
                var testSkillScore = EnsureTestSkillScore(testSectionResult.SkillScores.ToList(), skillId.Value);

                var sectionAnswers = sectionGroup.ToList();
                for (int i = 0; i < sectionAnswers.Count; i++)
                {
                    var answer = sectionAnswers[i];

                    // run order riêng theo section (result bé)
                    var sectionRunOrder = i == 0 ? First_Run_Order : (i + 1);

                    // run order global theo skill (result lớn)
                    var globalRunOrder = NextGlobalRunOrder(globalRunOrderBySkill, skillId.Value);

                    await EvaluateSingleAnswerAndBlendAsync(
                        answer: answer,
                        aiConfig: aiConfig!,
                        wordContent: BuildWritingContent(answer),
                        sectionDisplayOrder: sectionDisplayOrder,
                        testResultId: testResult.Id,
                        sectionRunOrder: sectionRunOrder,
                        globalRunOrder: globalRunOrder,
                        sectionSkillScore: sectionSkillScore,
                        testSkillScore: testSkillScore,
                        studentEmail: student?.User?.Email,
                        testName: testResult.Test?.Name,
                        cancellationToken: cancellationToken);
                }

                // update Result bé
                currentSectionResult.SkillScores = sectionSkillScores;
                currentSectionResult.CorrectCount = (int)sectionSkillScore.CorrectCount;
                currentSectionResult.CorrectTotal = (int)sectionSkillScore.TotalCount;
                if (scoringFormulaType == EnumScoringFormulaType.Percent)
                {
                    var percent = testSectionResult.TestSection?.Percent ?? default;
                    currentSectionResult.PercentModule = NumberHelper.ConvertDoublePercent(percent * currentSectionResult.Percent);
                }
                else
                {
                    var scores = currentSectionResult.SkillScores[0].Scores;
                    var percent = currentSectionResult.TestSection?.Percent ?? default;
                    currentSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * percent, 2);
                }

                testSectionResult.SkillScores = new List<SkillScores> { testSkillScore };
                testSectionResult.CorrectCount = (int)testSkillScore.CorrectCount;
                testSectionResult.CorrectTotal = (int)testSkillScore.TotalCount;
            }
            if (scoringFormulaType == EnumScoringFormulaType.Percent)
            {
                var percent = testSectionResult.TestSection?.Percent ?? default;
                testSectionResult.PercentModule = NumberHelper.ConvertDoublePercent(sectionResults.Sum(x => x.PercentModule) * percent, 2);
            }
            else if (scoringFormulaType == EnumScoringFormulaType.BandScore)
            {
                var scores = testSectionResult.SkillScores[0].Scores;
                var percent = testSectionResult.TestSection?.Percent ?? default;
                testSectionResult.ScoreModule = NumberHelper.ConvertDoublePercent(scores * percent, 2);
            }

            // 7) Persist
            await PersistAnswersAsync(allAnswers, cancellationToken);
            await SaveTestSectionResultsAsync(sectionResults, cancellationToken);
            await UpdateTestResultIfDoneAsync(testResult, cancellationToken);
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

        // =========================
        // Load data
        // =========================

        private async Task<List<TestSectionResult>> LoadAllSectionResultsAsync(Guid testResultId, CancellationToken cancellationToken)
        {
            // TODO: đổi đúng field FK của bạn nếu không phải TestResultId
            return await _testSectionResultRepository.ReadQueryable
                .Include(x => x.TestSection)
                .Where(x => x.ParentTestSectionResultId == testResultId)
                .ToListAsync(cancellationToken);
        }

        private async Task<List<TestAnswer>> LoadAnswersBySectionResultIdsAsync(List<Guid> sectionResultIds, CancellationToken cancellationToken)
        {
            return await _testAnswerRepository.Queryable
                .Include(x => x.TestSection)
                .Where(x => x.TestSectionResultId.HasValue && sectionResultIds.Contains(x.TestSectionResultId.Value))
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

        // =========================
        // Core evaluate + blend (Result bé + Result lớn)
        // =========================

        private async Task EvaluateSingleAnswerAndBlendAsync(
            TestAnswer answer,
            TestAISetting aiConfig,
            string wordContent,
            int sectionDisplayOrder,
            Guid testResultId,
            int sectionRunOrder,
            int globalRunOrder,
            SkillScores sectionSkillScore,
            SkillScores testSkillScore,
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

            // 7) Tính điểm
            (double averageScore, double totalScore) = CalculateOverallAverage(
                parsed.TaskResponse!,
                parsed.Coherence!,
                parsed.LexicalResource!,
                parsed.GrammaticalRange!);

            // 8) Blend vào Result bé (theo section run order)
            BlendWritingScore(sectionSkillScore, totalScore);

            BlendWritingScore(testSkillScore, averageScore, totalScore, globalRunOrder);
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

                    var aiResponse = await SendChatGPT(aiConfig, item.UserRoleStr!, userAiConfig, cancellationToken);
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
            var json = ConvertHelper.Serialize(feedback);
            var dict = ConvertHelper.Deserialize<Dictionary<string, string>>(json);

            var taskResponse = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("taskResponse"));
            var coherence = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("coherence"));
            var lexical = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("lexicalResource"));
            var grammar = ConvertHelper.Deserialize<List<TestAIGradingModel>>(dict?.GetValueOrDefault("grammaticalRange"));

            var ok = taskResponse != null && coherence != null && lexical != null && grammar != null;

            return (taskResponse, coherence, lexical, grammar, ok);
        }

        private static void BlendWritingScore(SkillScores skillScore, double totalScore)
        {
            if (skillScore.TotalCount == 0)
            {
                skillScore.TotalCount = CorrectTotal_Writing;
            }
            skillScore.TotalQuestion = 1;
            skillScore.CorrectCount = totalScore;
            skillScore.Scores = NumberHelper.ConvertRound(totalScore / 4, 2);
        }

        private static void BlendWritingScore(SkillScores skillScore, double averageScore, double totalScore, int runOrder)
        {
            if (skillScore.TotalCount == 0)
            {
                skillScore.TotalCount = CorrectTotal_Writing;
                skillScore.CorrectCount = totalScore;
                skillScore.Scores = averageScore;
                skillScore.TotalQuestion = 2;
                skillScore.CountQuestion = 2;
            }
            else
            {
                skillScore.CorrectCount = (int)CaculateAverageScoreWritingSection(skillScore.CorrectCount, totalScore, runOrder);
                skillScore.Scores = CaculateAverageScoreWritingSection(skillScore.Scores, averageScore, runOrder);
            }
        }

        // =========================
        // Result lớn helpers
        // =========================

        private static SkillScores EnsureTestSkillScore(List<SkillScores> testSkillScores, Guid skillId)
        {
            var existed = testSkillScores.FirstOrDefault(x => x.SkillId == skillId);
            if (existed != null)
            {
                return existed;
            }

            var created = new SkillScores
            {
                SkillId = skillId,
                CorrectCount = 0,
                Scores = 0,
                TotalCount = CorrectTotal_Writing,
                TotalQuestion = 0,
                CountQuestion = 0,
            };

            testSkillScores.Add(created);
            return created;
        }

        private static int NextGlobalRunOrder(Dictionary<Guid, int> globalRunOrderBySkill, Guid skillId)
        {
            if (!globalRunOrderBySkill.TryGetValue(skillId, out var current))
            {
                globalRunOrderBySkill[skillId] = 1;
                return 1;
            }

            var next = current + 1;
            globalRunOrderBySkill[skillId] = next;
            return next;
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

        // =========================
        // Persist
        // =========================

        private async Task PersistAnswersAsync(List<TestAnswer> answers, CancellationToken cancellationToken)
        {
            await _testAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                _testAnswerRepository.UpdateList(answers);
                await _testAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return new MethodResult<bool>();
            });
        }

        private async Task SaveTestSectionResultsAsync(List<TestSectionResult> sectionResults, CancellationToken cancellationToken)
        {
            await _testSectionResultRepository.BulkUpdateList(sectionResults, bulk =>
            {
                bulk.ColumnInputExpression = c => new { c.SkillScoresStr, c.CorrectTotal, c.CorrectCount, c.Percent, c.PercentModule };
            });
        }

        // =========================
        // SkillScores helpers
        // =========================

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

        private static string BuildWritingContent(TestAnswer answer)
        {
            // TODO: đổi sang field writing thật của bạn nếu không phải SpeechTextAnswer
            return answer.AnswerStr ?? string.Empty;
        }

        // =========================
        // Calculate
        // =========================

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

        // =========================
        // AI send + websocket
        // =========================

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
