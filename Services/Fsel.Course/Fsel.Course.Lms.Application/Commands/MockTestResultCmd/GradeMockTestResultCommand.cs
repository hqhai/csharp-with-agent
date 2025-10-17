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
    using Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1;
    using Fsel.Course.Lms.Application.InternalEvents;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IMediator _mediator;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly MockTestResultInputThenUpdateUnitResultHandler _mockTestResultInputThenUpdateUnitResultHandler;
        private readonly ICourseRepository _courseRepository;
        private static int Criteria = 4;

        public GradeMockTestResultCommandHandler(IMapper mapper,
            IMediator mediator,
            IMockTestResultRepository mockTestResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IUserService userService,
            NotificationMessagePublisher notificationMessagePublisher,
            AuthContext authContext,
            MockTestResultInputThenUpdateUnitResultHandler mockTestResultInputThenUpdateUnitResultHandler,
            ICourseRepository courseRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mockTestResultInputThenUpdateUnitResultHandler = mockTestResultInputThenUpdateUnitResultHandler;
            _courseRepository = courseRepository;
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
            //if (mockTestResult.MockTestScores.Count > 0)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(mockTestResult.MockTestScores));
            //    return methodResult;
            //}
            var sectionGroupIds = request.MockTestScores.Select(y => y.SectionGroupId).Distinct().ToList();

            var sectionGroups = await _sectionGroupRepository.Queryable.Where(x => sectionGroupIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (sectionGroups.Count != sectionGroupIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroups));
                return methodResult;
            }
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).WhereBulkContains(sectionGroupIds, x => x.SectionGroupId).Where(x => x.MockTestResultId == mockTestResult.Id && x.CreatedDate >= mockTestResult.CreatedDate).ToListAsync(cancellationToken);
            if (sectionGroupResults == null || !sectionGroupResults.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResults));
                return methodResult;
            }
            var level = mockTestResult.Course.CourseLevel;

            #endregion Validate

            var isSkillTest = mockTestResult.MockTest.MockTestType == EnumMockTestType.SkillMockTest;

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

            mockTestResult.CorrectCount += sectionGroupResults.Sum(x => x.CorrectCount);
            mockTestResult.SkillScores = skillScores;
            mockTestResult.MockTestScores = mockTestScores;
            mockTestResult.GradingTeacherId = teacherId;

            await _mockTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionGroupResultRepository.BulkUpdateList(sectionGroupResults, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.WorkingTime, c.StudentId, c.SectionGroupId, c.PlacementTestResultId, c.MockTestResultId, c.FinalTestResultId };
                });
                await SendNotification(mockTestResult, cancellationToken);

                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.MockTestId, c.UnitId };
                });
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<MockTestScoreModel>>(mockTestResult.MockTestScores);
                return methodResult;
            });
            await _mediator.Send(new SendTokenHistoryCommand { MockTestResultId = mockTestResult.Id }, cancellationToken);

            if (mockTestResult.MockTest.MockTestType == EnumMockTestType.FullMockTest)
            {
                var course = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder)).ThenInclude(p => p.Unit).FirstOrDefaultAsync(p => p.Id == mockTestResult.CourseId, cancellationToken);

                if (course.CourseUnitMockTests.Where(p => p.MockTestId.HasValue).FirstOrDefault()?.MockTestId == mockTestResult.MockTestId)
                {
                    await _mockTestResultInputThenUpdateUnitResultHandler.SendMailMidCourseReport(mockTestResult.StudentId, course!, cancellationToken);
                }
            }
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
    }
}
