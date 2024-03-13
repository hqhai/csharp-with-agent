// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
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
        private readonly IMapper _mapper;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ISystemService _systemService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;

        public SendTokenHistoryCommandHandler(IMapper mapper, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestResultRepository mockTestResultRepository, ISectionGroupResultRepository sectionGroupResultRepository, IUserService userService, ICourseRepository courseRepository, ISystemService systemService, CreateTokenHistoryPublisher createTokenHistoryPublisher)
        {
            _mapper = mapper;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
            _courseRepository = courseRepository;
            _systemService = systemService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
        }

        public async Task<MethodResult<bool>> Handle(SendTokenHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
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
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { mockTestResult.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults.Content?.Result?.FirstOrDefault();
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

            var isSkillMockTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;
            if (mockTestResult.Status == EnumResultStatus.Done && (isSkillMockTest || mockTestResult.GradingTeacherId.HasValue))
            {
                await GetTokenHistoryAsync(mockTestResult, isSkillMockTest, student, cancellationToken);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task GetTokenHistoryAsync(MockTestResult mockTestResult, bool isSkillMockTest, StudentModel student, CancellationToken cancellationToken)
        {
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).ThenInclude(x => x!.Sections.OrderBy(x => x.DisplayOrder)).Where(x => x.MockTestResultId == mockTestResult.Id).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            var tokenHistoryQueues = new List<TokenHistoryQueueModel>();
            var userId = student.Human?.UserId ?? default;
            if (sectionGroupResults != null && sectionGroupResults.Any())
            {
                foreach (var item in sectionGroupResults)
                {
                    if (item.Status == EnumResultStatus.Done)
                    {
                        var listTokenHistory = await UpdateSectionGroupResultsAsync(item, isSkillMockTest, userId, cancellationToken);
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

            if (mockTestResult.TokenFirstTime.HasValue && mockTestResult.TokenFirstTime.Value > 0)
            {
                await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                {
                    NumberOfToken = mockTestResult.TokenFirstTime.Value,
                    StudentId = mockTestResult.StudentId,
                }).ConfigureAwait(false);
            }

            _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<IList<TokenHistoryQueueModel>> UpdateSectionGroupResultsAsync(SectionGroupResult? sectionGroupResult, Course course, Guid userId, bool isSkillMockTest, CancellationToken cancellationToken)
        {
            var tokenHistorys = new List<TokenHistoryQueueModel>();
            if (sectionGroupResult != null && sectionGroupResult.SectionGroup != null)
            {
                switch (sectionGroupResult.SectionGroup.CourseSkill)
                {
                    case EnumCourseSkill.Reading:
                    case EnumCourseSkill.Listening:
                        var tokenHistoryReadings = await UpdateSectionGroupResultAsync(sectionGroupResult, isSkillMockTest, userId, cancellationToken);
                        if (tokenHistoryReadings.Any())
                        {
                            tokenHistorys.AddRange(tokenHistoryReadings);
                        }
                        break;

                    case EnumCourseSkill.Writing:
                        var mocktestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id).ToListAsync(cancellationToken);
                        foreach (var section in sectionGroupResult.SectionGroup.Sections)
                        {
                            var mockTestAnswer = mocktestAnswers.FirstOrDefault(x => x.SectionId == section.Id);
                            if (mockTestAnswer != null)
                            {
                                await UpdateSectionGroupResultToWritingAsync(sectionGroupResult, section, course, mockTestAnswer, isSkillMockTest, userId);
                            }
                        }

                        break;

                    case EnumCourseSkill.Speaking:
                        break;
                }
            }

            return tokenHistorys;
        }

        #region Skill Reading And Listening

        private async Task<IList<TokenHistoryQueueModel>> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, bool isSkillTest, Guid userId, CancellationToken cancellationToken)
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
                        tokenHistorys.Add(GetTokenHistoryQueue(sectionGroupResult, userId, tokenMission.Value));
                    }
                }

                _sectionGroupResultRepository.Update(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
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

        private async Task<(SectionGroupResult, List<TokenHistoryQueueModel>)> UpdateSectionGroupResultToWritingAsync(SectionGroupResult sectionGroupResult, Section section, Course course, MockTestAnswer mockTestAnswer, bool checkSkillMockTest, Guid userId)
        {
            var tokenHistoryQueues = new List<TokenHistoryQueueModel>();
            var gradingAiFeedBackResult = ConvertHelper.Deserialize<MockTestGradingAiFeedBack>(mockTestAnswer.GradingAlFeedback);
            if (gradingAiFeedBackResult == null)
            {
                return (sectionGroupResult, tokenHistoryQueues);
            }
            EnumTokenMission misstionWork, misstionTaskResponse, misstionCoherence, misstionLexicalResource, misstionGrammaticalRange;
            if (section.DisplayOrder == 1)
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

            var tokenOverallWordContent = CalculateOverall(mockTestAnswer.Answer?.ToString(), section.DisplayOrder == 1 ? 150 : 250, tokenWork?.BaseValue);
            var tokenOverallTaskResponse = CalculateOverall(taskResponse, tokenTaskResponse?.BaseValue, course);
            var tokenOverallCoherence = CalculateOverall(coherence, tokenCoherence?.BaseValue, course);
            var tokenOverallLexicalResource = CalculateOverall(lexicalResource, tokenLexicalResource?.BaseValue, course);
            var tokenOverallGrammaticalRange = CalculateOverall(grammaticalRange, tokenGrammaticalRange?.BaseValue, course);

            var userId = student.Human?.UserId ?? default;
            if (checkSkillMockTest)
            {
                tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, userId, misstionWork, tokenOverallWordContent ?? default));
                tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, userId, misstionTaskResponse, tokenOverallTaskResponse ?? default));
                tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, userId, misstionCoherence, tokenOverallCoherence ?? default));
                tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, userId, misstionLexicalResource, tokenOverallLexicalResource ?? default));
                tokenHistoryQueues.Add(GetTokenHistoryQueue(sectionGroupResult, userId, misstionGrammaticalRange, tokenOverallGrammaticalRange ?? default));
            }

            var taskResponseToken = new List<double?> { tokenOverallWordContent, tokenOverallTaskResponse, tokenOverallCoherence, tokenOverallLexicalResource, tokenOverallGrammaticalRange };
            if (sectionGroupResult.TokenFirstTime.HasValue)
            {
                sectionGroupResult.TokenFirstTime += (int?)taskResponseToken.Sum();
            }
            else
            {
                sectionGroupResult.TokenFirstTime = (int?)taskResponseToken.Sum();
            }
            return (sectionGroupResult, tokenHistoryQueues);
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

        private static TokenHistoryQueueModel GetTokenHistoryQueue(SectionGroupResult sectionGroupResult, Guid userId, EnumTokenMission mission, double? token = null)
        {
            return new TokenHistoryQueueModel
            {
                ObjectId = sectionGroupResult.MockTestResultId,
                RemainToken = token ?? sectionGroupResult.TokenFirstTime ?? default,
                Type = EnumTokenHistoryType.Exchanged,
                Feature = EnumTokenFeature.SkillMockTest,
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
    }
}
