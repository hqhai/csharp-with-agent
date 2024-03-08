// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
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
        private readonly IUserService _userService;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISystemService _systemService;
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestAISettingRepository _aiGradeSettingRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMediator _mediator;
        private static int CorrecTotalWriting = 36;

        public SubmitMockTestAnswerCommandHandler(SubmitAIResponsePublisher submitAIResponsePublisher, IUserService userService, ISectionRepository sectionRepository, ISystemService systemService, ICourseRepository courseRepository, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestAISettingRepository aiGradeSettingRepository, ISectionGroupResultRepository sectionGroupResultRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _userService = userService;
            _sectionRepository = sectionRepository;
            _systemService = systemService;
            _courseRepository = courseRepository;
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

            var sectionGroupResult = _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefault(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId);

            var mockTestResult = _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefault(x => x.Id == request.MockTestResultId);

            if (mockTestResult == null || mockTestResult.MockTest == null)
            {
                return true;
            }
            var section = await _sectionRepository.GetByIdAsync(request.SectionId);
            if (section == null)
            {
                return true;
            }
            bool checkSkillMockTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;

            (double averageScore, double totalScore) = CalculateOverallAverage(taskResponse!, coherence!, lexicalResource!, grammaticalRange!);
            (bool isError, int token) = await GetTokenAsync(mockTestResult, section, checkSkillMockTest, request.WordContent, resultDictionary);

            if (isError)
            {
                return true;
            }
            var skillScore = sectionGroupResult!.SkillScores?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Writing);

            var skillScores = sectionGroupResult!.SkillScores?.ToList() ?? new List<SkillScores>();
            if (sectionGroupResult.TokenFirstTime.HasValue)
            {
                sectionGroupResult.TokenFirstTime += token;
            }
            else
            {
                sectionGroupResult.TokenFirstTime = token;
            }
            if (skillScore == null)
            {
                skillScore = new SkillScores
                {
                    CorrectCount = totalScore,
                    TotalCount = 36,
                    Skill = EnumCourseSkill.Writing,
                    Scores = averageScore,
                    TotalQuestion = 2,
                    CountQuestion = 2
                };
                skillScores.Add(skillScore);
            }
            else
            {
                skillScore = skillScores.Single();
                int correcCount = (int)CaculateAverageScoreWritingSection(skillScore!.CorrectCount, totalScore);
                averageScore = CaculateAverageScoreWritingSection(skillScore!.Scores, averageScore);
                skillScores.Single().CorrectCount = correcCount;
                skillScores.Single().Scores = averageScore;
                sectionGroupResult.CorrectCount = correcCount;

                if (checkSkillMockTest)
                {
                    mockTestResult.CorrectCount = correcCount;
                    mockTestResult.SkillScores = skillScores;
                    mockTestResult.CorrectTotal = CorrecTotalWriting;
                    mockTestResult.TokenFirstTime = sectionGroupResult.TokenFirstTime;
                }
                else
                {
                    mockTestResult.TokenFirstTime += sectionGroupResult.TokenFirstTime;
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
                    _mockTestResultRepository.Update(mockTestResult);
                    if (mockTestResult.TokenFirstTime.HasValue && mockTestResult.TokenFirstTime != 0)
                    {
                        await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                        {
                            NumberOfToken = mockTestResult.TokenFirstTime.Value,
                            StudentId = mockTestResult.StudentId,
                        }).ConfigureAwait(false);

                        await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = gradingAiFeedBack,
            }, cancellationToken);

            return true;
        }

        private async Task<(bool, int)> GetTokenAsync(MockTestResult mockTestResult, Section section, bool checkSkillMockTest, string? wordContent, Dictionary<EnumMockTestAIType, string> resultDictionary)
        {
            var course = await _courseRepository.GetByIdAsync(mockTestResult.CourseId);
            if (course == null)
            {
                return (true, default);
            }
            var listMisstion = new List<string>();
            if (section.DisplayOrder == 1)
            {
                listMisstion = checkSkillMockTest ? new List<string> { nameof(EnumTokenMission.SkillMockTestWritingTask1CC), nameof(EnumTokenMission.SkillMockTestWritingTask1GRA), nameof(EnumTokenMission.SkillMockTestWritingTask1LR), nameof(EnumTokenMission.SkillMockTestWritingTask1TA), nameof(EnumTokenMission.SkillMockTestWritingTask1Work150) }
            : new List<string> { nameof(EnumTokenMission.FullMockTestWritingTask1CC), nameof(EnumTokenMission.FullMockTestWritingTask1GRA), nameof(EnumTokenMission.FullMockTestWritingTask1LR), nameof(EnumTokenMission.FullMockTestWritingTask1TA), nameof(EnumTokenMission.FullMockTestWritingTask1Work150) };
            }
            else
            {
                listMisstion = checkSkillMockTest ? new List<string> { nameof(EnumTokenMission.SkillMockTestWritingTask2CC), nameof(EnumTokenMission.SkillMockTestWritingTask2GRA), nameof(EnumTokenMission.SkillMockTestWritingTask2LR), nameof(EnumTokenMission.SkillMockTestWritingTask2TA), nameof(EnumTokenMission.SkillMockTestWritingTask2Work250) }
             : new List<string> { nameof(EnumTokenMission.FullMockTestWritingTask2CC), nameof(EnumTokenMission.FullMockTestWritingTask2GRA), nameof(EnumTokenMission.FullMockTestWritingTask2LR), nameof(EnumTokenMission.FullMockTestWritingTask2TA), nameof(EnumTokenMission.FullMockTestWritingTask2Work250) };
            }

            var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
            {
                CourseType = EnumCourseType.Ielts,
                Feature = checkSkillMockTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Missions = string.Join(",", listMisstion)
            });
            var tokenConfigs = tokenConfigResults.Content?.Result;

            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary[EnumMockTestAIType.TaskResponse],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };
            TokenCoinConfigs? tokenWork, tokenTaskResponse, tokenCoherence, tokenLexicalResource, tokenGrammaticalRange;
            if (section.DisplayOrder == 1)
            {
                tokenWork = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1Work150 : EnumTokenMission.FullMockTestWritingTask1Work150);
                tokenTaskResponse = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1TA : EnumTokenMission.FullMockTestWritingTask1TA);
                tokenCoherence = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1CC : EnumTokenMission.FullMockTestWritingTask1CC);
                tokenLexicalResource = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1LR : EnumTokenMission.FullMockTestWritingTask1LR);
                tokenGrammaticalRange = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1GRA : EnumTokenMission.FullMockTestWritingTask1GRA);
            }
            else
            {
                tokenWork = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2Work250 : EnumTokenMission.FullMockTestWritingTask2Work250);
                tokenTaskResponse = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2TA : EnumTokenMission.FullMockTestWritingTask2TA);
                tokenCoherence = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2CC : EnumTokenMission.FullMockTestWritingTask2CC);
                tokenLexicalResource = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2LR : EnumTokenMission.FullMockTestWritingTask2LR);
                tokenGrammaticalRange = GetTokenCoinConfig(tokenConfigs, checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2GRA : EnumTokenMission.FullMockTestWritingTask2GRA);
            }

            var taskResponse = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.TaskResponse);
            var coherence = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.Coherence);
            var lexicalResource = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.GrammaticalRange);

            var taskResponseToken = new List<double?> { CalculateOverall(wordContent,section.DisplayOrder == 1 ? 150 : 250 , tokenWork?.BaseValue), CalculateOverall(taskResponse, tokenTaskResponse?.BaseValue, course), CalculateOverall(coherence, tokenCoherence?.BaseValue, course)
                , CalculateOverall(lexicalResource, tokenLexicalResource?.BaseValue, course), CalculateOverall(grammaticalRange, tokenGrammaticalRange?.BaseValue, course) };

            return (false, (int)(taskResponseToken.Sum() ?? default));
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

        private static double CaculateAverageScoreWritingSection(double firstScore, double average)
        {
            return NumberHelper.RoundNumberDouble((firstScore + average * 2) / 3);
        }

        private static TokenCoinConfigs? GetTokenCoinConfig(IList<TokenConfigModel>? tokenConfigs, EnumTokenMission mission)
        {
            return tokenConfigs?.FirstOrDefault(x => x.Mission == mission).GetTokenConfig<TokenCoinConfigs>();
        }

        private static double? CalculateOverall(IList<MockTestAIGradingModel>? bandScoreDescriptions, double? baseValue, Course course)
        {
            var bandScore = course.CourseLevel.GetBandScore();
            var score = CaculateAverageScore(bandScoreDescriptions?.ToList());
            return score >= bandScore - 1 ? baseValue : default;
        }

        private static double? CalculateOverall(string? wordContent, int minimumNumberOfWords, double? baseValue)
        {
            var numberOfWord = Shared.Helpers.StringHelper.CountWords(wordContent);
            return numberOfWord >= minimumNumberOfWords ? baseValue : default;
        }
    }
}
