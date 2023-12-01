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
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetMockTestResultByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, SectionConverter sectionConverter, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestResultByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);
            mockTest = await GetMockTestAsync(mockTest, mockTestResult);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }
            var isCheckFull = mockTest.MockTestType == EnumMockTestType.FullMockTest;
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
            mockTestResult = _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = GetMockTestDto(mockTest, mockTestResult, isCheckFull);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MockTest?> GetMockTestAsync(MockTest? mockTest, MockTestResult mockTestResult)
        {
            if (mockTest != null)
            {
                var query = _mockTestRepository.Queryable.Include(x => x!.MockTestSections)
                                              .ThenInclude(x => x.SectionGroup)
                                              .ThenInclude(x => x!.Sections)
                                              .ThenInclude(x => x.SectionTimeCodes)
                                              .ThenInclude(x => x.MockTestAnswers.Where(x => x.MockTestResultId == mockTestResult.Id))
                                          .Include(x => x!.MockTestSections)
                                              .ThenInclude(x => x.SectionGroup)
                                              .ThenInclude(x => x!.Sections)
                                              .ThenInclude(x => x.MockTestAnswers.Where(x => x.MockTestResultId == mockTestResult.Id))
                                          .Include(x => x!.MockTestSections)
                                              .ThenInclude(x => x.SectionGroup)
                                              .ThenInclude(x => x!.MockTestScores.Where(x => x.MockTestResultId == mockTestResult.Id));

                if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
                {
                    mockTest = await query.Include(x => x.CourseUnitMockTests)
                                              .ThenInclude(x => x.Course)
                                          .Where(x => x.Id == mockTestResult.MockTestId)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();
                }
                else
                {
                    mockTest = await query.Include(x => x.UnitSkillMockTests)
                                              .ThenInclude(x => x.Unit)
                                              .ThenInclude(x => x!.CourseUnitMockTests)
                                              .ThenInclude(x => x.Course)
                                           .Where(x => x.Id == mockTestResult.MockTestId)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();
                }
            }
            return mockTest;
        }

        private MockTestResultModel GetMockTestResult(MockTestResult mockTestResult, MockTest mockTest, bool isCheckFull)
        {
            var mockTestResultDto = _mapper.Map<MockTestResultModel>(mockTestResult);
            mockTestResultDto.Scores = mockTestResult.SkillScores != null ? mockTestResult.SkillScores.Average(x => x.Scores) : 0;
            var courseUnitMockTests = mockTest.UnitSkillMockTests.Where(x => x.UnitId == mockTestResult.UnitId).Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests);
            mockTestResultDto.UnitDisplayOrder = isCheckFull ? mockTest.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault() : courseUnitMockTests.Select(x => x.Number).FirstOrDefault();
            mockTestResultDto.CourseCode = isCheckFull ? mockTest.CourseUnitMockTests.Select(x => x.Course?.Code).FirstOrDefault() : courseUnitMockTests.Select(x => x.Course?.Code).FirstOrDefault();
            return mockTestResultDto;
        }

        private MockTestModel GetMockTestDto(MockTest? mockTest, MockTestResult? mockTestResult, bool isCheckFull)
        {
            ArgumentNullException.ThrowIfNull(mockTest);
            ArgumentNullException.ThrowIfNull(mockTestResult);
            var mockTestDto = _mapper.Map<MockTestModel>(mockTest);
            mockTestDto.IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any();
            mockTestDto.SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                    .Select(x => x.SectionGroup)
                    .Where(x => !isCheckFull || (x!.CourseSkill != EnumCourseSkill.Reading || x.CourseSkill != EnumCourseSkill.Listening))
                    .Where(x => x!.CourseSkill == EnumCourseSkill.Speaking || x.CourseSkill == EnumCourseSkill.Writing)
                    .OrderBy(x => x!.CreatedDate)
                    .Select(x => _sectionConverter.GetSectionGroupModel(x, mockTestResult.Status)).ToList();
            mockTestDto.MockTestResult = GetMockTestResult(mockTestResult, mockTest, isCheckFull);

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
