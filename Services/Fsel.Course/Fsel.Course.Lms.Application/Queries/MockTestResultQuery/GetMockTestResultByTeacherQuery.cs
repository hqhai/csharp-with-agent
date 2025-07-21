// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestResultByTeacherQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestResultByTeacherQueryHandler : IRequestHandler<GetMockTestResultByTeacherQuery, MethodResult<MockTestModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetMockTestResultByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMockTestRepository mockTestRepository, SectionGroupConverter sectionGroupConverter, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mockTestRepository = mockTestRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestResultByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);
            (mockTest, int displayOrder, string? code) = await GetMockTestAsync(mockTest, mockTestResult);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;
            if (mockTestResult.GradingStartDate.HasValue && mockTestResult.GradingStartDate.Value.AddMinutes(30) < DateTime.UtcNow)
            {
                mockTestResult.GradingTeacherId = null;
                mockTestResult.GradingStartDate = null;
            }
            else if (mockTestResult.GradingTeacherId == null)
            {
                mockTestResult.GradingTeacherId = teacherId;
                mockTestResult.GradingStartDate = DateTime.UtcNow;
            }
            await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
            });
            methodResult.Result = GetMockTestDto(mockTest, mockTestResult, displayOrder, code);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<(MockTest?, int, string?)> GetMockTestAsync(MockTest? mockTest, MockTestResult mockTestResult)
        {
            mockTest = await _mockTestRepository.Queryable.Include(x => x!.MockTestSections)
                                               .ThenInclude(x => x.SectionGroup)
                                               .ThenInclude(x => x!.Sections)
                                               .ThenInclude(x => x.SectionTimeCodes)
                                               .ThenInclude(x => x.MockTestAnswers.Where(x => x.MockTestResultId == mockTestResult.Id))
                                               .Where(x => x.Id == mockTestResult.MockTestId)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();
            if (mockTest != null)
            {
                var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable
                                                              .Where(x => (mockTestResult.UnitId.HasValue ? x.UnitId == mockTestResult.UnitId : x.MockTestId == mockTestResult.MockTestId) && x.CourseId == mockTestResult.CourseId)
                                                              .Select(x => new
                                                              {
                                                                  UnitDisplayOrder = x.Number,
                                                                  CourseCode = x.Course!.Code
                                                              }).FirstOrDefaultAsync();
                return (mockTest, courseUnitMockTest?.UnitDisplayOrder ?? default, courseUnitMockTest?.CourseCode);
            }

            return (mockTest, default, string.Empty);
        }

        private MockTestResultModel GetMockTestResult(MockTestResult mockTestResult, MockTest mockTest, int displayOrder, string? code)
        {
            var mockTestResultDto = _mapper.Map<MockTestResultModel>(mockTestResult);
            mockTestResultDto.Scores = mockTestResult.SkillScores != null ? mockTestResult.SkillScores.Average(x => x.Scores) : 0;
            var courseUnitMockTests = mockTest.UnitSkillMockTests.Where(x => x.UnitId == mockTestResult.UnitId).Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests);
            mockTestResultDto.UnitDisplayOrder = displayOrder;
            mockTestResultDto.CourseCode = code;
            mockTestResultDto.MockTestScores = mockTestResult.MockTestScores;
            return mockTestResultDto;
        }

        private MockTestModel GetMockTestDto(MockTest? mockTest, MockTestResult? mockTestResult, int displayOrder, string? code)
        {
            ArgumentNullException.ThrowIfNull(mockTest);
            ArgumentNullException.ThrowIfNull(mockTestResult);
            var mockTestDto = _mapper.Map<MockTestModel>(mockTest);
            mockTestDto.IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any();
            mockTestDto.SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                    .Select(x => x.SectionGroup)
                    .Where(x => !(mockTest.MockTestType == EnumMockTestType.FullMockTest) || x!.CourseSkill == EnumCourseSkill.Speaking)
                    .OrderBy(x => x!.CreatedDate)
                    .Select(x => _sectionGroupConverter.GetSectionGroupModel(x, false)).ToList();
            mockTestDto.MockTestResult = GetMockTestResult(mockTestResult, mockTest, displayOrder, code);

            if (mockTestDto.MockTestType == EnumMockTestType.SkillMockTest)
            {
                mockTestDto.PostArea = "U" + mockTestDto.MockTestResult.UnitDisplayOrder + "_" + mockTestDto.MockTestResult.CourseCode;
            }
            else
            {
                mockTestDto.PostArea = "FM" + mockTestDto.MockTestResult.UnitDisplayOrder + "_" + mockTestDto.MockTestResult.CourseCode;
            }
            return mockTestDto;
        }
    }
}
