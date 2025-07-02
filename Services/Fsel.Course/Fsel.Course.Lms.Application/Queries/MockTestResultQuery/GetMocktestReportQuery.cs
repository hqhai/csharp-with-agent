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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
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
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private float Default_Achieved_Point = 1;
        private const double Standard_Ratio = 1; // tỉ lệ xem đánh giá 100/100

        public GetMockTestReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, SectionGroupConverter sectionGroupConverter, AuthContext authContext, IUserService userService, IMapper mapper, QuestBoardPublisher questBoardPublisher)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(GetMockTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();

            var mockTestResult = await _mockTestResultRepository.Queryable
                                    .Include(x => x.MockTestScores)
                                    .ThenInclude(x => x.SectionGroup)
                                    .Where(x => x.Id == request.MockTestResultId)
                                    .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var mockTest = await _mockTestRepository.Queryable
                                        .Include(x => x!.MockTestSections)
                                        .ThenInclude(x => x.SectionGroup)
                                    .FirstOrDefaultAsync(cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            if (mockTestResult.MockTestScores != null && mockTestResult.MockTestScores.Any())
            {
                mockTestResult.IsViewed = true;
                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
                });
            }

            var mockTestResultModel = GetMockTestResult(mockTestResult);
            if (mockTestResult.SkillScores != null && mockTestResult.SkillScores.Any())
            {
                mockTestResultModel.Scores = NumberHelper.RoundNumberDouble(mockTestResult.SkillScores.Average(x => x.Scores));
            }
            if (mockTest.MockTestSections.Any())
            {
                mockTestResultModel.IsTeacherGraded = await _sectionGroupConverter.IsTeacherGraded(mockTestResult, mockTest.MockTestSections.Select(x => x.SectionGroup!.CourseSkill).ToList());
            }

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
                                            MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(n.Select(m => m.MockTestScore).OrderBy(x => x.CreatedDate).ToList())
                                        }).ToList();

            return mockTestResult;
        }
    }
}
