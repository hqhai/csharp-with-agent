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
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public GradeMockTestResultCommandHandler(IMapper mapper,
            IMockTestResultRepository mockTestResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            IUserService userService,
            AuthContext authContext,
            NotificationMessagePublisher notificationMessagePublisher)
        {
            _mapper = mapper;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _userService = userService;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<List<MockTestScoreModel>>> Handle(GradeMockTestResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<MockTestScoreModel>> methodResult = new MethodResult<List<MockTestScoreModel>>();

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

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(e => e.Id == request.MockTestResultId).FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
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
                var listMockTestScore = mockTestScores.Where(x => x.SectionGroupId == item.Id).ToList();
                var sumScore = listMockTestScore.Sum(x => x.Score);
                var skillScore = mockTestResult.SkillScores?.FirstOrDefault(x => x.Skill == item.CourseSkill);
                if (skillScore != null)
                {
                    skillScore.CorrectCount = sumScore;
                    skillScore.TotalCount = 36;
                    skillScore.Skill = item.CourseSkill;
                    skillScore.Scores = NumberHelper.RoundNumberDouble((double)sumScore / listMockTestScore.Count, true);
                    skillScores!.Add(skillScore);
                }
            }

            mockTestResult.SkillScores = skillScores;
            mockTestResult.MockTestScores = mockTestScores;
            mockTestResult.GradingTeacherId = teacherId;

            await _mockTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
                NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
                {
                    ObjectId = mockTestResult!.Id,
                    UserIds = new List<Guid> { mockTestResult.StudentId },
                    Type = EnumNotificationType.LinkPage,
                    Content = EnumNotificationContent.MockTest,
                    PlatformCode = EnumPlatformCode.LMS,
                    ParamsMessage = new List<object> { mockTestResult.MockTest!.Name ?? string.Empty },
                };
                await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
            }
        }
    }
}
