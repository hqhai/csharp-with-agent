// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Constants;
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
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly IMockTestAISettingRepository _aiGradeSettingRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMediator _mediator;
        public SubmitMockTestAnswerCommandHandler(SubmitAIResponsePublisher submitAIResponsePublisher, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestAISettingRepository aiGradeSettingRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _mediator = mediator;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<bool> Handle(SubmitMockTestAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var mockTestAnswer = _mockTestAnswerRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId && x.MockTestResultId == request.MockTestResultId);

            var aiConfig = _aiGradeSettingRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId);


            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            foreach (var item in aiConfig!.Prompts!)
            {
                //var answer = string.Concat(aiConfig.Task!, item.PromptContent!);

                var answer = string.Concat(new string[] { aiConfig.Task!, Environment.NewLine, item.PromptContent! });

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

                resultDictionary[item.Type] = aIResponse!;
            }

            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary[EnumMockTestAIType.TaskResponse],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };

            var taskResponse = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.TaskResponse);
            var coherence = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.Coherence);
            var lexicalResource = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.GrammaticalRange);

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(e => e.Id == request.MockTestResultId).FirstOrDefaultAsync(cancellationToken);

            if (mockTestResult == null)
            {
                return true;
            }

            var skillScores = mockTestResult.SkillScores?.Where(x => !(x.Skill == EnumCourseSkill.Writing)).ToList();

            double averageScore = CalculateOverallAverage(taskResponse!, coherence!, lexicalResource!, grammaticalRange!);

            var skillScore = mockTestResult.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);

            if (skillScore != null && skillScore.SaveFirstTime)
            {
                averageScore = CaculateAverageScoreWritingSection(skillScore, averageScore);
                skillScore.CorrectCount = averageScore;
                skillScore.TotalCount = 36;
                skillScore.Skill = EnumCourseSkill.Writing;
                skillScore.Scores = averageScore;
                skillScores!.Add(skillScore);

            }
            else if (skillScore != null && !skillScore.SaveFirstTime)
            {
                skillScore.CorrectCount = averageScore;
                skillScore.TotalCount = 36;
                skillScore.Skill = EnumCourseSkill.Writing;
                skillScore.Scores = averageScore;
                skillScore.SaveFirstTime = true;
                skillScores!.Add(skillScore);
            }

            mockTestResult.SkillScores = skillScores;

            string? gradingAiFeedBack = ConvertHelper.Serialize(gradingAiFeedBackResult);

            if (mockTestAnswer != null)
            {
                mockTestAnswer.GradingAlFeedback = gradingAiFeedBack;

                _mockTestAnswerRepository.Update(mockTestAnswer);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = gradingAiFeedBack,
            }, cancellationToken);

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



        private static double CalculateOverallAverage(params List<MockTestAIGradingModel>[] bandScoreDescriptions)
        {
            double totalScore = 0;

            foreach (var bandScoreDescription in bandScoreDescriptions)
            {
                totalScore += CaculateAverageScore(bandScoreDescription);
            }

            double average = totalScore / bandScoreDescriptions.Length;

            return NumberHelper.RoundNumberDouble(average, true);
        }


        private static double CaculateAverageScoreWritingSection(SkillScores skillScore, double average)
        {
            return NumberHelper.RoundNumberDouble((skillScore.Scores + average * 2) / 3, true);
        }

    }
}
