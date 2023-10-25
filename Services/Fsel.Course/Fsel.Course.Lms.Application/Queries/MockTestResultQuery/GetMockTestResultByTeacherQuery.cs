// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetMockTestResultByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, SectionConverter sectionConverter, AuthContext authContext, IUserService userService, ICourseRepository courseRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _authContext = authContext;
            _userService = userService;
            _courseRepository = courseRepository;
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
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults)
                                    .Include(x => x!.MockTestSections)
                                        .ThenInclude(x => x.SectionGroup)
                                        .ThenInclude(x => x!.Sections)
                                        .ThenInclude(x => x.SectionTimeCodes)
                                        .ThenInclude(x => x.MockTestAnswers)
                                     .Include(x => x!.MockTestSections)
                                        .ThenInclude(x => x.SectionGroup)
                                        .ThenInclude(x => x!.Sections)
                                        .ThenInclude(x => x.MockTestAnswers)
                                     .Include(x => x!.MockTestSections)
                                        .ThenInclude(x => x.SectionGroup)
                                        .ThenInclude(x => x!.MockTestScores)
                                        .Include(x => x!.MockTestResults)
                                        .ThenInclude(x => x.Course)
                                        .ThenInclude(x => x!.CourseUnitMockTests)
                                    .Where(x => x.Id == mockTestResult.MockTestId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);

            var isCheckFull = mockTest!.MockTestType == EnumMockTestType.FullMockTest;

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;

            if (mockTestResult.GradingStartDate.HasValue && mockTestResult.GradingStartDate.Value.AddMinutes(30) < DateTime.UtcNow)
            {
                mockTestResult.GradingTeacherId = null;
                mockTestResult.GradingStartDate = null;
            }
            else
            {
                if (mockTestResult.GradingTeacherId == null)
                {
                    mockTestResult.GradingTeacherId = teacherId;
                    mockTestResult.GradingStartDate = DateTime.UtcNow;
                }
            }
            var mockTestModel = new MockTestModel
            {
                Id = mockTest!.Id,
                Name = mockTest.Name,
                MockTestType = mockTest.MockTestType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any(),
                SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup)
                         .Where(x => !isCheckFull || (x!.CourseSkill != EnumCourseSkill.Reading || x.CourseSkill != EnumCourseSkill.Listening))
                         .Where(x => x!.CourseSkill == EnumCourseSkill.Speaking || x.CourseSkill == EnumCourseSkill.Writing)
                         .OrderBy(x => x!.CreatedDate)
                         .Select(x => _sectionConverter.GetSectionGroupModel(x, false)).ToList(),
                MockTestResult = mockTest.MockTestResults.Where(x => x.Id == mockTestResult.Id)
                .Select(x =>
                {
                    var result = _mapper.Map<MockTestResultModel>(x);
                    result.Scores = x.SkillScores != null ? x.SkillScores.Average(x => x.Scores) : 0;
                    result.UnitDisplayOrder = x.MockTest!.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault();
                    result.CourseCode = x.MockTest.MockTestResults.Select(x => x.Course?.Code).FirstOrDefault();
                    result.GradingStartDate = mockTestResult.GradingStartDate;
                    result.GradingTeacherId = mockTestResult.GradingTeacherId;
                    return result;
                }).FirstOrDefault()
            };

            if (mockTestModel.MockTestType == EnumMockTestType.SkillMockTest)
            {
                mockTestModel.PostArea = "U" + mockTestModel.MockTestResult?.UnitDisplayOrder + "_" + mockTestModel.MockTestResult?.CourseCode;
            }
            else
            {
                mockTestModel.PostArea = "FM" + mockTestModel.MockTestResult?.UnitDisplayOrder + "_" + mockTestModel.MockTestResult?.CourseCode;
            }
            mockTestResult = _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = mockTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
