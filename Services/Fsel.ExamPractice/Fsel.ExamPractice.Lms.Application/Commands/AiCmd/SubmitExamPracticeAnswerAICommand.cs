// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.AiCmd
{
    using System;
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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

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
        private readonly IMapper _mapper;
        private const int CorrectTotal_Writing = 36;
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
            IMediator mediator, IMapper mapper)
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
            var examPracticeAnswer = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeSectionResultId == request.ExamPracticeSectionResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken);

            var aiConfig = await _aiGradeSettingRepository.Queryable.Include(x => x.ExamPracticeAICriteriaSettings)
                                                                    .FirstOrDefaultAsync(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId, cancellationToken);

            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            var examPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(request.ExamPracticeSectionId);
            if (examPracticeSection == null)
            {
                return true;
            }

            if (aiConfig == null || aiConfig.ExamPracticeAICriteriaSettings == null && aiConfig.SystemRoleAlConfig == null || aiConfig!.ExamPracticeAICriteriaSettings!.Count == 0 && aiConfig.SystemRoleAlConfig == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(aiConfig.SystemRoleAlConfig) || aiConfig.Prompts != null && aiConfig.Prompts.Count == 0)
            {
                foreach (var item in aiConfig.ExamPracticeAICriteriaSettings)
                {
                    var answer = string.Concat(new string[] { aiConfig!.Task!, Environment.NewLine, item.Prompts!.Single().PromptContent! });

                    var userAiConfig = answer?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                    if (userAiConfig == null)
                    {
                        return false;
                    }

                    var aIResponse = await SendChatGPT(aiConfig, item.SystemRoleAlConfig!, userAiConfig, cancellationToken);

                    resultDictionary[item.Prompts![0].Type] = aIResponse!;

                    await SendWebSocket(aIResponse, item.Prompts![0].Type.ToString(), examPracticeSection.DisplayOrder, examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
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

                    await SendWebSocket(aIResponse, item.Type.ToString(), examPracticeSection.DisplayOrder, examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
                }
            }

            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary.ContainsKey(EnumMockTestAIType.TaskResponse) ? resultDictionary[EnumMockTestAIType.TaskResponse] : resultDictionary[EnumMockTestAIType.TaskAchievement],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };

            var taskResponse = gradingAiFeedBackResult.TaskResponse.Deserialize<List<ExamPracticeAIGradingModel>>();
            var coherence = gradingAiFeedBackResult.Coherence.Deserialize<List<ExamPracticeAIGradingModel>>();
            var lexicalResource = gradingAiFeedBackResult.LexicalResource.Deserialize<List<ExamPracticeAIGradingModel>>();
            var grammaticalRange = gradingAiFeedBackResult.GrammaticalRange.Deserialize<List<ExamPracticeAIGradingModel>>();

            var examPracticeResult = await _examPracticeResultRepository.Queryable.Include(x => x.ExamPractice)
                                                                        .FirstOrDefaultAsync(x => x.Id == examPracticeSectionResult.ExamPracticeResultId, cancellationToken);
            if (examPracticeResult == null || examPracticeResult.ExamPractice == null)
            {
                return true;
            }

            var studentResults = await _userService.GetUserByStudentIdWithCache(examPracticeResult.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                return true;
            }
            var student = studentResults.Content?.Result;
            if (student == null)
            {
                return true;
            }

            #region Retry

            if (examPracticeAnswer != null && (taskResponse == null || coherence == null || lexicalResource == null || grammaticalRange == null) && examPracticeAnswer.RetryTime <= Max_Times_Retry)
            {
                var model = _mapper.Map<SetTimeRetryExamPracticeModel>(request);
                model.StartDate = DateTime.UtcNow;
                await _setTimeRetryExamPracticePublisher.Publish(model, cancellationToken);
                examPracticeAnswer.RetryTime += 1;
            }

            #endregion Retry

            bool checkSkillMockTest = examPracticeResult.ExamPractice.SubType == EnumExamPracticeSubType.SkillMockTest;

            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse!, coherence!, lexicalResource!, grammaticalRange!);

            var skillScore = examPracticeSectionResult.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);
            var skillScores = examPracticeSectionResult.SkillScores?.ToList() ?? new List<SkillScores>();

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
                int correcCount = (int)CaculateAverageScoreWritingSection(skillScore!.CorrectCount, totalScore, examPracticeSection.DisplayOrder);
                averageScore = CaculateAverageScoreWritingSection(skillScore!.Scores, averageScore, examPracticeSection.DisplayOrder);
                skillScores.Single().CorrectCount = correcCount;
                skillScores.Single().Scores = averageScore;
                examPracticeSectionResult.CorrectCount = correcCount;

                if (checkSkillMockTest)
                {
                    examPracticeResult.CorrectCount = correcCount;
                    examPracticeResult.SkillScores = skillScores;
                    examPracticeResult.CorrectTotal = CorrectTotal_Writing;
                }
            }

            examPracticeSectionResult.SkillScores = skillScores;
            string? gradingAiFeedBack = gradingAiFeedBackResult.Serialize();

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

            return true;
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

            double.TryParse(firstItem.BandScore, out bandScore);

            return bandScore;
        }

        private static (double average, double totalScore) CalculateOverallAverage(params List<ExamPracticeAIGradingModel>[] bandScoreDescriptions)
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
                SettingModel = aiConfig.SettingModel,
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
