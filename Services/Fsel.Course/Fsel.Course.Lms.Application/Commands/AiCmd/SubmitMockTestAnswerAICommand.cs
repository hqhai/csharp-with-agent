// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly IMediator _mediator;
        private const int CorrectTotal_Writing = 36;
        private const int Last_DisplayOrder = 1;


        public SubmitMockTestAnswerCommandHandler(SubmitMockTestCriteriaPublisher submitMockTestCriteria, IUserService userService, ISectionRepository sectionRepository, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestAISettingRepository aiGradeSettingRepository, ISectionGroupResultRepository sectionGroupResultRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _submitMockTestCriteria = submitMockTestCriteria;
            _userService = userService;
            _sectionRepository = sectionRepository;
            _mediator = mediator;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<bool> Handle(SubmitMockTestAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var mockTestAnswer = _mockTestAnswerRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId && x.MockTestResultId == request.MockTestResultId);

            var aiConfigs = _aiGradeSettingRepository.Queryable.Where(x => x.SectionId == request.SectionId).ToList();

            var aiConfig = _aiGradeSettingRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId);

            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            var section = await _sectionRepository.GetByIdAsync(request.SectionId);
            if (section == null)
            {
                return true;
            }

            foreach (var item in aiConfigs)
            {
                //var answer = string.Concat(aiConfig.Task!, item.PromptContent!);

                var answer = string.Concat(new string[] { aiConfig!.Task!, Environment.NewLine, item.Prompts!.Single().PromptContent! });

                var userAiConfig = answer?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                if (userAiConfig == null)
                {
                    return false;
                }

                var aIResponse = await _mediator.Send(new SubmitAICommand
                {
                    SettingModel = aiConfig?.SettingModel,
                    SettingTemperature = aiConfig!.SettingTemperature,
                    SettingFrequecy = aiConfig!.SettingFrequecy,
                    SettingWordMaxLength = aiConfig!.SettingWordMaxLength,
                    SettingPresence = aiConfig!.SettingPresence,
                    SettingTopP = aiConfig!.SettingTopP,
                    SystemRoleAlConfig = aiConfig!.SystemRoleAlConfig,
                    UserAIConfig = userAiConfig,
                }, cancellationToken).ConfigureAwait(false);

                resultDictionary[item.Criteria] = aIResponse!;

                await _submitMockTestCriteria.Publish(new SubmitMockTestResponseModel
                {
                    GradingAlFeedBack = aIResponse,
                    CriteriaName = item.Criteria.ToString(),
                    DisplayOrder = section.DisplayOrder,
                    MockTestResultId = request.MockTestResultId
                }, cancellationToken);
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
            var sectionGroupResult = _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefault(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId);
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


            bool checkSkillMockTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;

            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse!, coherence!, lexicalResource!, grammaticalRange!);

            var skillScore = sectionGroupResult!.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);

            var skillScores = sectionGroupResult!.SkillScores?.ToList() ?? new List<SkillScores>();

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
                _mockTestAnswerRepository.Update(mockTestAnswer);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _sectionGroupResultRepository.Update(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (checkSkillMockTest)
                {
                    var sections = await _sectionRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
                    _mockTestResultRepository.Update(mockTestResult);
                    if (skillScore != null)
                    {
                        await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        await _mediator.Send(new SendTokenHistoryCommand { MockTestResultId = mockTestResult.Id }, cancellationToken);
                    }
                    else
                    {
                        await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
    }
}
