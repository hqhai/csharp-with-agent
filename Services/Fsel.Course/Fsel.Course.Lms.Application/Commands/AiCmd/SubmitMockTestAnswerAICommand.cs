// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubmitMockTestAnswerAICommand : MockTestAnswerResponseModel, IRequest<bool>
    {
    }

    public class SubmitMockTestAnswerCommandHandler : IRequestHandler<SubmitMockTestAnswerAICommand, bool>
    {
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly SubmitMockTestCriteriaPublisher _submitMockTestCriteria;
        private readonly IUserService _userService;
        private readonly ISectionRepository _sectionRepository;
        private readonly IMockTestAISettingRepository _aiGradeSettingRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly SetTimeRetryMockTestPublisher _setTimeRetryMockTestPublisher;
        private readonly ISenderService _senderService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private const int CorrectTotal_Writing = 36;
        private const int Last_DisplayOrder = 1;
        private const int Max_Times_Retry = 3;

        public SubmitMockTestAnswerCommandHandler(SubmitMockTestCriteriaPublisher submitMockTestCriteria, IUserService userService, ISectionRepository sectionRepository, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestAISettingRepository aiGradeSettingRepository, ISectionGroupResultRepository sectionGroupResultRepository, IMockTestResultRepository mockTestResultRepository, SetTimeRetryMockTestPublisher setTimeRetryMockTestPublisher, IMapper mapper, AppSetting appSetting, ISenderService senderService)
        {
            _submitMockTestCriteria = submitMockTestCriteria;
            _userService = userService;
            _sectionRepository = sectionRepository;
            _mediator = mediator;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _setTimeRetryMockTestPublisher = setTimeRetryMockTestPublisher;
            _mapper = mapper;
            _appSetting = appSetting;
            _senderService = senderService;
        }

        public async Task<bool> Handle(SubmitMockTestAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var mockTestAnswer = _mockTestAnswerRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId && x.MockTestResultId == request.MockTestResultId);

            var aiConfig = _aiGradeSettingRepository.Queryable.Include(x => x.MockTestAICriteriaSettings).FirstOrDefault(x => x.SectionId == request.SectionId);

            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            var section = await _sectionRepository.GetByIdAsync(request.SectionId);
            if (section == null)
            {
                return true;
            }

            if (aiConfig == null || (aiConfig.MockTestAICriteriaSettings == null && aiConfig.SystemRoleAlConfig == null || aiConfig!.MockTestAICriteriaSettings!.Count == 0 && aiConfig.SystemRoleAlConfig == null))
            {
                return true;
            }

            if (string.IsNullOrEmpty(aiConfig.SystemRoleAlConfig) || (aiConfig.Prompts != null && aiConfig.Prompts.Count == 0))
            {
                foreach (var item in aiConfig.MockTestAICriteriaSettings)
                {
                    //var answer = string.Concat(aiConfig.Task!, item.PromptContent!);

                    var answer = string.Concat(new string[] { aiConfig!.Task!, Environment.NewLine, item.Prompts!.Single().PromptContent! });

                    var userAiConfig = answer?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                    if (userAiConfig == null)
                    {
                        return false;
                    }

                    var aIResponse = await SendChatGPT(aiConfig, item.SystemRoleAlConfig!, userAiConfig, cancellationToken);

                    resultDictionary[item.Prompts![0].Type] = aIResponse!;

                    await SendWebSocket(aIResponse, item.Prompts![0].Type.ToString(), section.DisplayOrder, request.MockTestResultId, cancellationToken);
                }
            }
            else
            {
                foreach (var item in aiConfig.Prompts!)
                {
                    //var answer = string.Concat(aiConfig.Task!, item.PromptContent!);

                    var answer = string.Concat(new string[] { aiConfig.Task!, Environment.NewLine, item.PromptContent! });

                    var userAiConfig = answer?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                    if (userAiConfig == null)
                    {
                        return false;
                    }

                    var aIResponse = await SendChatGPT(aiConfig, aiConfig.SystemRoleAlConfig, userAiConfig, cancellationToken);

                    resultDictionary[item.Type] = aIResponse!;

                    await SendWebSocket(aIResponse, item.Type.ToString(), section.DisplayOrder, request.MockTestResultId, cancellationToken);
                }
            }

            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse) ? resultDictionary[EnumMockTestAIType.TaskResponse] : resultDictionary[EnumMockTestAIType.TaskAchievement],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };

            var taskResponse = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.TaskResponse);
            var coherence = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.Coherence);
            var lexicalResource = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.GrammaticalRange);

            var mockTestResult = _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefault(x => x.Id == request.MockTestResultId);
            if (mockTestResult == null || mockTestResult.MockTest == null)
            {
                return true;
            }
            var sectionGroupResult = _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefault(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId && x.CreatedDate >= mockTestResult.CreatedDate);
            if (sectionGroupResult == null)
            {
                return true;
            }

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { mockTestResult.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                return true;
            }
            var student = studentResults.Content?.Result?.FirstOrDefault();
            if (student == null)
            {
                return true;
            }

            #region Retry

            //set up thời gian retry
            if (mockTestAnswer.RetryTime > Max_Times_Retry)
            {
                SendEmailCommandModel model = new SendEmailCommandModel
                {
                    ToEmails = new List<string> { _appSetting!.CustomerSupportConfig!.Email! },
                    CcEmails = _appSetting!.CustomerSupportConfig!.CCEmail!,
                    Content = ValueSettings.CustomerSupport.Content.Format(student.User?.Email ?? default, mockTestResult.MockTest.Name),
                    Subject = ValueSettings.CustomerSupport.TitleMail.Format(student.User?.Email ?? default),
                };

                await _senderService.SendEmailAsync(model);
            }
            else if (mockTestAnswer != null && (taskResponse == null || coherence == null || lexicalResource == null || grammaticalRange == null) && mockTestAnswer.RetryTime <= Max_Times_Retry)
            {
                var model = _mapper.Map<SetTimeRetryMockTestModel>(request);
                model.StartDate = DateTime.UtcNow;
                await _setTimeRetryMockTestPublisher.Publish(model, cancellationToken);
                mockTestAnswer.RetryTime += 1;
            }

            // Nếu là last mocktest mà retry 3 lần gpt vẫn chưa cho về kết quả thì khóa luồng, không cho đi tiếp, còn nếu không thì luồng vẫn done và học sinh có thể tiếp tục
            var lastMockTest = mockTestResult.MockTest.CourseUnitMockTests
                     .Where(x => x.MockTestId == mockTestResult.MockTestId)
                     .OrderByDescending(x => x.DisplayOrder)
                     .FirstOrDefault();

            if (lastMockTest != null && mockTestAnswer!.RetryTime > Max_Times_Retry)
            {
                mockTestResult.Status = EnumResultStatus.Unfinished;
            }

            #endregion Retry

            bool checkSkillMockTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;

            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse!, coherence!, lexicalResource!, grammaticalRange!);

            var skillScore = sectionGroupResult.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);
            var skillScores = sectionGroupResult.SkillScores?.ToList() ?? new List<SkillScores>();

            if (skillScore == null)
            {
                skillScore = new SkillScores
                {
                    CorrectCount = totalScore,
                    Skill = EnumCourseSkill.Writing,
                    Scores = averageScore,
                    TotalQuestion = 2,
                    CountQuestion = 2,
                    TotalCount = CorrectTotal_Writing,
                };

                skillScores.Add(skillScore);
            }
            else
            {
                skillScore = skillScores.Single();
                int correcCount = (int)CaculateAverageScoreWritingSection(skillScore!.CorrectCount, totalScore, section.DisplayOrder);
                averageScore = CaculateAverageScoreWritingSection(skillScore!.Scores, averageScore, section.DisplayOrder);
                skillScores.Single().CorrectCount = correcCount;
                skillScores.Single().Scores = averageScore;
                sectionGroupResult.CorrectCount = correcCount;

                if (checkSkillMockTest)
                {
                    mockTestResult.CorrectCount = correcCount;
                    mockTestResult.SkillScores = skillScores;
                    mockTestResult.CorrectTotal = CorrectTotal_Writing;
                }
            }

            sectionGroupResult.SkillScores = skillScores;
            string? gradingAiFeedBack = ConvertHelper.Serialize(gradingAiFeedBackResult);

            if (mockTestAnswer != null)
            {
                mockTestAnswer.GradingAlFeedback = gradingAiFeedBack;

                await _mockTestAnswerRepository.BulkUpdateList(new List<MockTestAnswer> { mockTestAnswer }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.MockTestResultId, c.SectionGroupResultId, c.SectionQuestionId, c.SectionId, c.SectionTimeCodeId };
                });

                await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.WorkingTime, c.SectionGroupId, c.PlacementTestResultId, c.MockTestResultId, c.FinalTestResultId, c.StudentId };
                });

                if (checkSkillMockTest)
                {
                    var sections = await _sectionRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
                    await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.MockTestId, c.CourseId, c.UnitId, c.StudentId };
                    });
                    if (skillScore != null)
                    {
                        await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        await _mediator.Send(new SendTokenHistoryCommand { MockTestResultId = mockTestResult.Id }, cancellationToken);
                    }
                }
            }

            return true;
        }

        private static double CaculateAverageScore(List<MockTestAIGradingModel>? bandScoreDescription)
        {
            double bandScore = 0;

            if (bandScoreDescription == null || bandScoreDescription.Count == 0)
            {
                return bandScore;
            }

            MockTestAIGradingModel firstItem = bandScoreDescription.FirstOrDefault()!;

            if (firstItem == null || firstItem.BandScore == null || firstItem.BandScore == string.Empty)
            {
                return bandScore;
            }

            double.TryParse(firstItem.BandScore, out bandScore);

            return bandScore;
        }

        private static (double average, double totalScore) CalculateOverallAverage(params List<MockTestAIGradingModel>[] bandScoreDescriptions)
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

        private async Task<string> SendChatGPT(MockTestAISetting aiConfig, string systemRole, string userAiConfig, CancellationToken cancellationToken)
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
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result))
            {
                aIResponse = result;
            }
            return aIResponse;
        }

        private async Task SendWebSocket(string aIResponse, string type, int displayOrder, Guid mockTestResultId, CancellationToken cancellationToken)
        {
            await _submitMockTestCriteria.Publish(new SubmitMockTestResponseModel
            {
                GradingAlFeedBack = aIResponse,
                CriteriaName = type,
                DisplayOrder = displayOrder,
                MockTestResultId = mockTestResultId
            }, cancellationToken);
        }
    }
}