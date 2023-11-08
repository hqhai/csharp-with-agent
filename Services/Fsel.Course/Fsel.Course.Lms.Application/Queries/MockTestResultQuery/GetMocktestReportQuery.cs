// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestReportQuery : IRequest<MethodResult<MockTestResultModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestReportQueryHandler : IRequestHandler<GetMockTestReportQuery, MethodResult<MockTestResultModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public GetMockTestReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, AuthContext authContext, IUserService userService, IMapper mapper, QuestBoardPublisher questBoardPublisher)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(GetMockTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var mockTestResult = await _mockTestResultRepository.Queryable
                                    .Include(x => x.MockTestScores)
                                        .ThenInclude(x => x.SectionGroup)
                                    .Where(x => x.Id == request.MockTestResultId && x.StudentId == studentId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            if (mockTestResult.MockTestScores != null)
            {
                mockTestResult.IsViewed = true;

                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            var mockTestResultModel = GetMockTestResult(mockTestResult);
            if (mockTestResult.SkillScores != null)
            {
                mockTestResultModel.Scores = NumberHelper.RoundNumberDouble(mockTestResult.SkillScores.Average(x => x.Scores), true);
                mockTestResultModel.IsTeacherGraded = await IsTeacherGraded(mockTestResult);
            }

            await DoQuestBoard(request.MockTestResultId, cancellationToken);

            methodResult.Result = mockTestResultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private MockTestResultModel GetMockTestResult(MockTestResult? x)
        {
            var mockTestResult = _mapper.Map<MockTestResultModel>(x);
            mockTestResult.MockTestScores = x?.MockTestScores
                                        .OrderBy(x => x.CreatedDate)
                                        .Where(n => n.SectionGroup != null)
                                        .Select(n => new
                                        {
                                            Skill = n.SectionGroup!.CourseSkill,
                                            MockTestScore = n
                                        })
                                        .GroupBy(n => n.Skill)
                                        .Select(n => new
                                        {
                                            Skill = n.Key,
                                            MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(n.Select(m => m.MockTestScore).ToList())
                                        });

            return mockTestResult;
        }

        private async Task<bool> IsTeacherGraded(MockTestResult data)
        {
            var mockTest = await _mockTestRepository.Queryable
                .Include(x => x.MockTestSections)
                .ThenInclude(x => x.SectionGroup)
                .Where(x => x.Id == data.MockTestId).FirstOrDefaultAsync();

            var skills = mockTest?.MockTestSections.Select(x => x.SectionGroup!.CourseSkill).ToList();
            if (skills != null && skills.Any(x => x == EnumCourseSkill.Speaking || x == EnumCourseSkill.Writing))
            {
                if (data.MockTestScores != null && data.MockTestScores.Any())
                {
                    var skillScores = data.MockTestScores.Select(x => x.SectionGroup!.CourseSkill).ToList();
                    return skillScores.Any(x => x == EnumCourseSkill.Speaking || x == EnumCourseSkill.Writing);
                }
                return false;
            }
            return true;
        }

        public async Task DoQuestBoard(Guid mockTestResultId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.SeeFiveTeacherReview, EnumQuestBoardCategory.SeeTenTeacherReview };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            var archievePoint = await _mockTestResultRepository.Queryable
                            .Where(x => x.Id == mockTestResultId && x.IsViewed && x.StudentId == studentId)
                            .ToListAsync(cancellationToken);
            var archievePointCount = archievePoint.Count;

            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentId = (Guid)studentId!,
                Categories = categories,
                AchievedPoint = archievePointCount,
            }, cancellationToken);
        }
    }
}
