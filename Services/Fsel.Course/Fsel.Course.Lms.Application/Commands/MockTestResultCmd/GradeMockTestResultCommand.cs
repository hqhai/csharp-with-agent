// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
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
    using Microsoft.EntityFrameworkCore;

    public class GradeMockTestResultCommand : GradeMockTestResultCommandModel, IRequest<MethodResult<List<MockTestScoreModel>>>
    {
    }

    public class GradeMockTestResultCommandHandler : IRequestHandler<GradeMockTestResultCommand, MethodResult<List<MockTestScoreModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private static int Criteria = 4;

        public GradeMockTestResultCommandHandler(IMapper mapper,
            IMockTestResultRepository mockTestResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IUserService userService,
            NotificationMessagePublisher notificationMessagePublisher,
            ISystemService systemService,
            AuthContext authContext)
        {
            _mapper = mapper;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
            _systemService = systemService;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<List<MockTestScoreModel>>> Handle(GradeMockTestResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<MockTestScoreModel>> methodResult = new MethodResult<List<MockTestScoreModel>>();

            #region Validate

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            var teacherId = teacherResult.Content?.Result?.Id;
            if (request.MockTestScores == null || request.MockTestScores.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.MockTestScores));
                return methodResult;
            }

            request.MockTestScores = request.MockTestScores.OrderBy(x => x.Criteria).ToList();
            if (request.MockTestScores.GroupBy(x => x.Criteria).Any(x => x.Count() > 1))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(MockTestScore.Criteria));
                return methodResult;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Include(x => x.MockTest).Include(x => x.Course).Where(e => e.Id == request.MockTestResultId).FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null || (mockTestResult.MockTest == null || mockTestResult.Course == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            if (mockTestResult.MockTestScores.Count > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(mockTestResult.MockTestScores));
                return methodResult;
            }
            var sectionGroupIds = request.MockTestScores.Select(y => y.SectionGroupId).Distinct().ToList();

            var sectionGroups = await _sectionGroupRepository.Queryable.Where(x => sectionGroupIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (sectionGroups.Count != sectionGroupIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroups));
                return methodResult;
            }
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(x => sectionGroupIds.Contains(x.SectionGroupId) && x.MockTestResultId == mockTestResult.Id).ToListAsync(cancellationToken);
            if (sectionGroupResults == null || !sectionGroupResults.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResults));
                return methodResult;
            }
            var level = mockTestResult.Course.CourseLevel;

            #endregion Validate

            var isSkillTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;
            var tokenConfigs = await GetTokenConfigsAsync(isSkillTest);

            long numberOfToken = 0;
            IList<MockTestScore> mockTestScores = new List<MockTestScore>();
            foreach (var item in request.MockTestScores)
            {
                if (item.Score > 9)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.ScoreMustLessThan9), nameof(item.Score), new
                    {
                        Score = item.Score,
                    });
                    return methodResult;
                }
                switch (item.Criteria)
                {
                    case EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy:
                        var mission = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingGRA : EnumTokenMission.FullMockTestSpeakingGRA;
                        var tokenConfig = tokenConfigs?.FirstOrDefault(x => x.Mission == mission);
                        numberOfToken += GetBandScore(level, tokenConfig.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        break;

                    case EnumMockTestScoreCriteria.Pronunciation:
                        var missionPon = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingPron : EnumTokenMission.FullMockTestSpeakingPron;
                        var tokenConfigPon = tokenConfigs?.FirstOrDefault(x => x.Mission == missionPon);
                        numberOfToken += GetBandScore(level, tokenConfigPon.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        break;

                    case EnumMockTestScoreCriteria.FluencyAndCoherence:
                        var missionFC = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingFC : EnumTokenMission.FullMockTestSpeakingFC;
                        var tokenConfigFC = tokenConfigs?.FirstOrDefault(x => x.Mission == missionFC);
                        numberOfToken += GetBandScore(level, tokenConfigFC.GetTokenConfig<TokenCoinConfigs>(), item.Score);

                        break;

                    case EnumMockTestScoreCriteria.LexicalResource:
                        var missionLR = isSkillTest ? EnumTokenMission.SkillMockTestSpeakingLR : EnumTokenMission.FullMockTestSpeakingLR;
                        var tokenConfigLR = tokenConfigs?.FirstOrDefault(x => x.Mission == missionLR);
                        numberOfToken += GetBandScore(level, tokenConfigLR.GetTokenConfig<TokenCoinConfigs>(), item.Score);
                        break;
                }

                MockTestScore mockTestScore = _mapper.Map<MockTestScore>(item);
                if (!mockTestScore.IsValid())
                {
                    methodResult.AddErrorBadRequest(mockTestScore.ErrorMessages);
                    return methodResult;
                }
                mockTestScores.Add(mockTestScore);
            }
            var skillScores = mockTestResult.SkillScores?.Where(x => !sectionGroups.Select(x => x.CourseSkill).Contains(x.Skill)).ToList();
            foreach (var item in sectionGroups)
            {
                var sectionGroupResult = sectionGroupResults.FirstOrDefault(x => x.SectionGroupId == item.Id);

                var listMockTestScore = mockTestScores.Where(x => x.SectionGroupId == item.Id).ToList();
                var sumScore = listMockTestScore.Sum(x => x.Score);
                var skillScore = mockTestResult.SkillScores?.FirstOrDefault(x => x.Skill == item.CourseSkill);
                if (sectionGroupResult != null)
                {
                    sectionGroupResult.CorrectCount = (int)sumScore;
                    sectionGroupResult.TokenFirstTime = (int?)numberOfToken;
                    sectionGroupResult.SkillScores = sectionGroupResult.SkillScores?.Select(x =>
                    {
                        x.CorrectCount = (int)sumScore;
                        x.Scores = NumberHelper.RoundNumberDouble((double)sumScore / Criteria);
                        return x;
                    }).ToList();
                }
                if (skillScore != null)
                {
                    skillScore.CorrectCount = sumScore;
                    skillScore.TotalCount = 36;
                    skillScore.Skill = item.CourseSkill;
                    skillScore.Scores = NumberHelper.RoundNumberDouble((double)sumScore / Criteria);
                    skillScores!.Add(skillScore);
                }
            }

            mockTestResult.SkillScores = skillScores;
            mockTestResult.MockTestScores = mockTestScores;
            mockTestResult.GradingTeacherId = teacherId;
            mockTestResult.TokenFirstTime += (int?)numberOfToken;

            await _mockTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                _sectionGroupResultRepository.UpdateList(sectionGroupResults);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                // Update Token To Student
                await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                {
                    NumberOfToken = mockTestResult.TokenFirstTime ?? default,
                    StudentId = mockTestResult.StudentId,
                }).ConfigureAwait(false);

                await SendNotification(mockTestResult, cancellationToken);
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<MockTestScoreModel>>(mockTestResult.MockTestScores);
                return methodResult;
            });
            return methodResult;
        }


        public async Task SendNotification(MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            if (mockTestResult != null)
            {
                bool isSkillMockTest = mockTestResult.MockTest!.MockTestType == EnumMockTestType.SkillMockTest;
                NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
                {
                    ObjectId = mockTestResult!.Id,
                    UserIds = new List<Guid> { mockTestResult.StudentId },
                    Type = EnumNotificationType.LinkPage,
                    Content = isSkillMockTest ? EnumNotificationContent.MockTest : EnumNotificationContent.FullMockTest,
                    PlatformCode = EnumPlatformCode.LMS,
                    ParamsMessage = new List<object> { mockTestResult.MockTest!.Name ?? string.Empty },
                    ParamsLink = BuildParamOfMockTestType(mockTestResult, isSkillMockTest),
                };
                await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
            }
        }

        private static List<object> BuildParamOfMockTestType(MockTestResult mockTestResult, bool isSkillMockTest)
        {
            var result = new List<object>();
            string sectionGroupId = mockTestResult?.SectionGroupResults.Select(x => x.SectionGroupId).Single().ToString() ?? string.Empty;
            string mockTestResulId = mockTestResult?.Id.ToString() ?? string.Empty;
            string mockTestId = mockTestResult?.MockTest?.Id.ToString() ?? string.Empty;
            string courseId = mockTestResult?.CourseId.ToString() ?? string.Empty;
            string unitId = mockTestResult?.UnitId.ToString() ?? string.Empty;

            if (isSkillMockTest)
            {
                result.AddRange(new object[] {
                        sectionGroupId,
                        mockTestResulId,
                        courseId,
                        unitId
                });
            }
            {
                result.AddRange(new object[] {
                        mockTestId,
                        courseId
                });
            }

            return result;
        }

        private static long GetBandScore(EnumCourseLevel level, TokenCoinConfigs? configs, double score)
        {
            var bandScore = level.GetBandScore();
            return score >= bandScore - 1 && configs != null ? configs.BaseValue : default;
        }

        private async Task<IList<TokenConfigModel>?> GetTokenConfigsAsync(bool isSkillTest)
        {
            var missions = isSkillTest ? new List<string> { nameof(EnumTokenMission.SkillMockTestSpeakingFC), nameof(EnumTokenMission.SkillMockTestSpeakingGRA), nameof(EnumTokenMission.SkillMockTestSpeakingLR), nameof(EnumTokenMission.SkillMockTestSpeakingPron) }
            : new List<string> { nameof(EnumTokenMission.FullMockTestSpeakingFC), nameof(EnumTokenMission.FullMockTestSpeakingGRA), nameof(EnumTokenMission.FullMockTestSpeakingLR), nameof(EnumTokenMission.FullMockTestSpeakingPron) };

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
    }
}
