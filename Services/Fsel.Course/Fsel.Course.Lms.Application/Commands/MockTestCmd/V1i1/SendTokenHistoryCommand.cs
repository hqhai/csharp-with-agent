// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SendTokenHistoryCommand : IRequest<MethodResult<bool>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class SendTokenHistoryCommandHandler : IRequestHandler<SendTokenHistoryCommand, MethodResult<bool>>
    {
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public SendTokenHistoryCommandHandler(IMockTestAnswerRepository mockTestAnswerRepository,
            ICourseResultRepository courseResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IUserService userService,
            ICourseRepository courseRepository,
            ISystemService systemService,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            ISectionGroupRepository sectionGroupRepository)
        {
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _systemService = systemService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendTokenHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores.OrderBy(x => x.CreatedDate)).Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            if (mockTestResult.MockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult.MockTest));
                return methodResult;
            }
            var studentResult = await _userService.GetUserByStudentId(mockTestResult.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(mockTestResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var sectionGroupCourseSkills = await _sectionGroupRepository.Queryable.Where(x => x.MockTestSections.Any(y => y.MockTestId == mockTestResult.MockTestId))
                                                                             .Select(x => x.CourseSkill)
                                                                             .ToListAsync(cancellationToken);

            var isSkillMockTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;
            var isSendToken = true;
            if (sectionGroupCourseSkills.Any())
            {
                if (isSkillMockTest && sectionGroupCourseSkills.Any(x => x == EnumCourseSkill.Writing || x == EnumCourseSkill.Speaking))
                {
                    isSendToken = mockTestResult.SkillScores?.Any(x => x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking) ?? default;
                }
                if (!isSkillMockTest)
                {
                    isSendToken = new[] { EnumCourseSkill.Writing, EnumCourseSkill.Speaking }.All((mockTestResult.SkillScores ?? new List<SkillScores>()).Select(x => x.Skill).Contains);
                }
            }

            if (mockTestResult.Status == EnumResultStatus.Done && isSendToken)
            {
                await GetTokenHistoryAsync(mockTestResult, course, student, isSkillMockTest, cancellationToken);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task GetTokenHistoryAsync(MockTestResult mockTestResult, Course course, StudentModel student, bool isSkillMockTest, CancellationToken cancellationToken)
        {
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).ThenInclude(x => x!.Sections.OrderBy(x => x.DisplayOrder)).Where(x => x.MockTestResultId == mockTestResult.Id && x.CreatedDate >= mockTestResult.CreatedDate).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            var tokenHistoryQueues = new List<TokenHistoryQueueModel>();
            var userId = student.Human?.UserId ?? default;
            if (sectionGroupResults != null && sectionGroupResults.Any())
            {
                foreach (var item in sectionGroupResults)
                {
                    if (item.Status == EnumResultStatus.Done)
                    {
                        var listTokenHistory = await UpdateSectionGroupResultsAsync(mockTestResult, item, course, userId, isSkillMockTest, cancellationToken);
                        if (listTokenHistory.Any())
                        {
                            tokenHistoryQueues.AddRange(listTokenHistory);
                        }
                    }
                }
                if (tokenHistoryQueues.Any())
                {
                    await _createTokenHistoryPublisher.Publish(tokenHistoryQueues, cancellationToken).ConfigureAwait(false);
                }
                await UpdateMockTestResultAsync(mockTestResult, sectionGroupResults, cancellationToken);
            }
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, IList<SectionGroupResult> sectionGroupResults, CancellationToken cancellationToken)
        {
            mockTestResult.TokenFirstTime = sectionGroupResults.Sum(x => (x.TokenFirstTime ?? default));
            await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.MockTestId, c.UnitId };
            });
        }

        private async Task<IList<TokenHistoryQueueModel>> UpdateSectionGroupResultsAsync(MockTestResult mockTestResult, SectionGroupResult? sectionGroupResult, Course course, Guid userId, bool isSkillMockTest, CancellationToken cancellationToken)
        {
            var tokenHistorys = new List<TokenHistoryQueueModel>();
            var courseResultId = _courseResultRepository.Queryable.FirstOrDefault(x => x.CourseId == mockTestResult.CourseId && x.StudentId == mockTestResult.StudentId)?.Id;
            if (sectionGroupResult != null && sectionGroupResult.SectionGroup != null)
            {
                switch (sectionGroupResult.SectionGroup.CourseSkill)
                {
                    case EnumCourseSkill.Reading:
                    case EnumCourseSkill.Listening:
                        var tokenHistoryReadings = await UpdateSectionGroupResultAsync(sectionGroupResult, courseResultId, isSkillMockTest, userId, cancellationToken);
                        if (tokenHistoryReadings.Any())
                        {
                            tokenHistorys.AddRange(tokenHistoryReadings);
                        }
                        break;

                    case EnumCourseSkill.Writing:
                        var mocktestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                             .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                             .ToListAsync(cancellationToken);
                        var tokenHistoryWritings = new List<TokenHistoryQueueModel>();
                        var sections = sectionGroupResult.SectionGroup.Sections.ToList();
                        foreach (var section in sections)
                        {
                            var mockTestAnswer = mocktestAnswers.FirstOrDefault(x => x.SectionId == section.Id);
                            if (mockTestAnswer != null)
                            {
                                var index = sections.IndexOf(section);
                                (sectionGroupResult, var listTokenHistory) = await UpdateSectionGroupResultToWritingAsync(sectionGroupResult, courseResultId, index, course, mockTestAnswer, isSkillMockTest, userId);
                                if (listTokenHistory.Any())
                                {
                                    tokenHistoryWritings.AddRange(listTokenHistory);
                                }
                            }
                        }
                        await UpdateSectionGroupResultAsync(sectionGroupResult, cancellationToken);
                        if (tokenHistoryWritings.Any())
                        {
                            tokenHistorys.AddRange(tokenHistoryWritings);
                        }
                        break;

                    case EnumCourseSkill.Speaking:
                        var tokenHistorySpeakings = new List<TokenHistoryQueueModel>();
                        var listTokenHistorySpeaking = await UpdateSectionGroupResultSpeakingAsync(mockTestResult, courseResultId, sectionGroupResult, course, userId, isSkillMockTest, cancellationToken);
                        if (listTokenHistorySpeaking.Any())
                        {
                            tokenHistorys.AddRange(listTokenHistorySpeaking);
                        }
                        break;
                }
            }

            return tokenHistorys.Where(x => x.VolatileToken > 0).ToList();
        }

        private async Task UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.WorkingTime, c.StudentId, c.SectionGroupId, c.PlacementTestResultId, c.MockTestResultId, c.FinalTestResultId };
            });
        }

        #region Skill Reading And Listening

        private async Task<IList<TokenHistoryQueueModel>> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, Guid? courseResultId, bool isSkillTest, Guid userId, CancellationToken cancellationToken)
        {
            EnumTokenMission? tokenMission = null;
            var tokenHistorys = new List<TokenHistoryQueueModel>();
            if (sectionGroupResult.SectionGroup != null)
            {
                if (sectionGroupResult.SectionGroup.CourseSkill == EnumCourseSkill.Reading)
                {
                    tokenMission = isSkillTest ? EnumTokenMission.SkillMockTestReading : EnumTokenMission.FullMockTestReading;
                }
                else if (sectionGroupResult.SectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    tokenMission = isSkillTest ? EnumTokenMission.SkillMockTestListening : EnumTokenMission.FullMockTestListening;
                }
                if (tokenMission.HasValue)
                {
                    var token = await GetTokenAsync(tokenMission.Value, isSkillTest);
                    sectionGroupResult.TokenFirstTime = (int?)token * sectionGroupResult.CorrectCount;
                    if (sectionGroupResult.TokenFirstTime.HasValue && sectionGroupResult.TokenFirstTime.Value > 0)
                    {
                        tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, tokenMission.Value, isSkillTest));
                    }
                }
                await UpdateSectionGroupResultAsync(sectionGroupResult, cancellationToken);
            }
            return tokenHistorys;
        }

        private async Task<long> GetTokenAsync(EnumTokenMission mission, bool isSkillTest)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = isSkillTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Mission = mission,
                CourseType = EnumCourseType.Ielts
            });
            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigs.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        #endregion Skill Reading And Listening

        #region Writing

        private async Task<(SectionGroupResult, List<TokenHistoryQueueModel>)> UpdateSectionGroupResultToWritingAsync(SectionGroupResult sectionGroupResult, Guid? courseResultId, int index, Course course, MockTestAnswer mockTestAnswer, bool checkSkillMockTest, Guid userId)
        {
            var tokenHistoryQueues = new List<TokenHistoryQueueModel>();
            var gradingAiFeedBackResult = ConvertHelper.Deserialize<MockTestGradingAiFeedBack>(mockTestAnswer.GradingAlFeedback);
            if (gradingAiFeedBackResult == null)
            {
                return (sectionGroupResult, tokenHistoryQueues);
            }
            EnumTokenMission misstionWork, misstionTaskResponse, misstionCoherence, misstionLexicalResource, misstionGrammaticalRange;
            if (index == 0)
            {
                misstionWork = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1Work150 : EnumTokenMission.FullMockTestWritingTask1Work150;
                misstionTaskResponse = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1TA : EnumTokenMission.FullMockTestWritingTask1TA;
                misstionCoherence = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1CC : EnumTokenMission.FullMockTestWritingTask1CC;
                misstionLexicalResource = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1LR : EnumTokenMission.FullMockTestWritingTask1LR;
                misstionGrammaticalRange = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask1GRA : EnumTokenMission.FullMockTestWritingTask1GRA;
            }
            else
            {
                misstionWork = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2Work250 : EnumTokenMission.FullMockTestWritingTask2Work250;
                misstionTaskResponse = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2TA : EnumTokenMission.FullMockTestWritingTask2TA;
                misstionCoherence = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2CC : EnumTokenMission.FullMockTestWritingTask2CC;
                misstionLexicalResource = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2LR : EnumTokenMission.FullMockTestWritingTask2LR;
                misstionGrammaticalRange = checkSkillMockTest ? EnumTokenMission.SkillMockTestWritingTask2GRA : EnumTokenMission.FullMockTestWritingTask2GRA;
            }
            var misstions = new List<EnumTokenMission> { misstionWork, misstionTaskResponse, misstionCoherence, misstionLexicalResource, misstionGrammaticalRange };
            List<string> listMisstion = misstions.Select(x => x.ToString()).ToList();
            var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
            {
                CourseType = EnumCourseType.Ielts,
                Feature = checkSkillMockTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Missions = string.Join(",", listMisstion)
            });
            var tokenConfigs = tokenConfigResults.Content?.Result;

            var tokenWork = GetTokenCoinConfig(tokenConfigs, misstionWork);
            var tokenTaskResponse = GetTokenCoinConfig(tokenConfigs, misstionTaskResponse);
            var tokenCoherence = GetTokenCoinConfig(tokenConfigs, misstionCoherence);
            var tokenLexicalResource = GetTokenCoinConfig(tokenConfigs, misstionLexicalResource);
            var tokenGrammaticalRange = GetTokenCoinConfig(tokenConfigs, misstionGrammaticalRange);

            var taskResponse = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.TaskResponse);
            var coherence = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.Coherence);
            var lexicalResource = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.LexicalResource);
            var grammaticalRange = ConvertHelper.Deserialize<List<MockTestAIGradingModel>>(gradingAiFeedBackResult.GrammaticalRange);

            var tokenOverallWordContent = CalculateOverall(mockTestAnswer.Answer?.ToString(), index == 0 ? 150 : 250, tokenWork?.BaseValue);
            var tokenOverallTaskResponse = CalculateOverall(taskResponse, tokenTaskResponse?.BaseValue, course);
            var tokenOverallCoherence = CalculateOverall(coherence, tokenCoherence?.BaseValue, course);
            var tokenOverallLexicalResource = CalculateOverall(lexicalResource, tokenLexicalResource?.BaseValue, course);
            var tokenOverallGrammaticalRange = CalculateOverall(grammaticalRange, tokenGrammaticalRange?.BaseValue, course);

            tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, misstionWork, checkSkillMockTest, tokenOverallWordContent ?? default));
            tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, misstionTaskResponse, checkSkillMockTest, tokenOverallTaskResponse ?? default));
            tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, misstionCoherence, checkSkillMockTest, tokenOverallCoherence ?? default));
            tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, misstionLexicalResource, checkSkillMockTest, tokenOverallLexicalResource ?? default));
            tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, misstionGrammaticalRange, checkSkillMockTest, tokenOverallGrammaticalRange ?? default));

            var taskResponseToken = new List<double?> { tokenOverallWordContent, tokenOverallTaskResponse, tokenOverallCoherence, tokenOverallLexicalResource, tokenOverallGrammaticalRange };
            if (sectionGroupResult.TokenFirstTime.HasValue)
            {
                sectionGroupResult.TokenFirstTime += (int?)taskResponseToken.Sum();
            }
            else
            {
                sectionGroupResult.TokenFirstTime = (int?)taskResponseToken.Sum();
            }
            return (sectionGroupResult, tokenHistoryQueues.Where(x => x.VolatileToken > 0).ToList());
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

        private static TokenHistoryQueueModel GetTokenHistoryQueue(SectionGroupResult sectionGroupResult, Guid? courseResultId, Guid userId, EnumTokenMission mission, bool isSkillMockTest, double? token = null)
        {
            return new TokenHistoryQueueModel
            {
                ObjectId = sectionGroupResult.MockTestResultId,
                VolatileToken = token ?? sectionGroupResult.TokenFirstTime ?? default,
                Type = EnumTokenHistoryType.Recevived,
                CourseResultId = courseResultId,
                Feature = isSkillMockTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                Mission = mission,
                UserId = userId,
            };
        }

        private class MockTestGradingAiFeedBack
        {
            public string? TaskResponse { get; set; }
            public string? Coherence { get; set; }
            public string? LexicalResource { get; set; }
            public string? GrammaticalRange { get; set; }
        }

        #endregion Writing

        #region Speaking

        private async Task<List<TokenHistoryQueueModel>> UpdateSectionGroupResultSpeakingAsync(MockTestResult mockTestResult, Guid? courseResultId, SectionGroupResult sectionGroupResult, Course course, Guid userId, bool isSkillTest, CancellationToken cancellationToken)
        {
            var tokenHistorys = new List<TokenHistoryQueueModel>();
            var level = course.CourseLevel;
            var tokenMissionFC = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingFC : EnumTokenMission.FullMockTestSpeakingFC;
            var tokenMissionGRA = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingGRA : EnumTokenMission.FullMockTestSpeakingGRA;
            var tokenMissionLR = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingLR : EnumTokenMission.FullMockTestSpeakingLR;
            var tokenMissionPron = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingPron : EnumTokenMission.FullMockTestSpeakingPron;

            var missions = new List<EnumTokenMission> { tokenMissionFC, tokenMissionGRA, tokenMissionLR, tokenMissionPron }.Select(x => x.ToString()).ToList();
            var tokenConfigs = await GetTokenConfigsAsync(isSkillTest, missions);
            sectionGroupResult.TokenFirstTime = 0;
            foreach (var item in mockTestResult.MockTestScores)
            {
                switch (item.Criteria)
                {
                    case EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy:
                        var tokenConfig = tokenConfigs?.FirstOrDefault(x => x.Mission == tokenMissionGRA);
                        var tokenGRA = GetBandScore(level, tokenConfig.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        if (tokenGRA > 0)
                        {
                            sectionGroupResult.TokenFirstTime += (int?)tokenGRA;
                            tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, tokenMissionGRA, isSkillTest, tokenGRA));
                        }

                        break;

                    case EnumMockTestScoreCriteria.Pronunciation:
                        var tokenConfigPon = tokenConfigs?.FirstOrDefault(x => x.Mission == tokenMissionPron);
                        var tokenPon = GetBandScore(level, tokenConfigPon.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        if (tokenPon > 0)
                        {
                            sectionGroupResult.TokenFirstTime += (int?)tokenPon;
                            tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, tokenMissionPron, isSkillTest, tokenPon));
                        }

                        break;

                    case EnumMockTestScoreCriteria.FluencyAndCoherence:
                        var tokenConfigFC = tokenConfigs?.FirstOrDefault(x => x.Mission == tokenMissionFC);
                        var tokenFC = GetBandScore(level, tokenConfigFC.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        if (tokenFC > 0)
                        {
                            sectionGroupResult.TokenFirstTime += (int?)tokenFC;
                            tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, tokenMissionFC, isSkillTest, tokenFC));
                        }
                        break;

                    case EnumMockTestScoreCriteria.LexicalResource:
                        var tokenConfigLR = tokenConfigs?.FirstOrDefault(x => x.Mission == tokenMissionLR);
                        var tokenLR = GetBandScore(level, tokenConfigLR.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        if (tokenLR > 0)
                        {
                            sectionGroupResult.TokenFirstTime += (int?)tokenLR;
                            tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, courseResultId, userId, tokenMissionLR, isSkillTest, tokenLR));
                        }
                        sectionGroupResult.TokenFirstTime += (int?)GetBandScore(level, tokenConfigLR.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        break;
                }
            }

            await UpdateSectionGroupResultAsync(sectionGroupResult, cancellationToken);
            return tokenHistorys;
        }

        private static long GetBandScore(EnumCourseLevel level, TokenCoinConfigs? configs, double score)
        {
            var bandScore = level.GetBandScore();
            return score >= bandScore - 1 && configs != null ? configs.BaseValue : default;
        }

        private async Task<IList<TokenConfigModel>?> GetTokenConfigsAsync(bool isSkillTest, List<string> missions)
        {
            var tokenConfigs = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
            {
                Feature = isSkillTest ? EnumTokenFeature.SkillMockTest : EnumTokenFeature.FullMockTest,
                CourseType = EnumCourseType.Ielts,
                Missions = string.Join(",", missions),
            });
            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }
            return tokenConfigs.Content?.Result;
        }

        #endregion Speaking
    }
}
