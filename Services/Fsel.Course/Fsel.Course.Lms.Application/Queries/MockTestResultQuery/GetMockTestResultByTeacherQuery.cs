// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
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

        public GetMockTestResultByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, SectionConverter sectionConverter, AuthContext authContext, IUserService userService)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _authContext = authContext;
            _userService = userService;
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
                                    .Where(x => x.Id == mockTestResult.MockTestId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);

            var isCheckFull = mockTest!.MockTestType == EnumMockTestType.FullMockTest;


            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;

            if (mockTestResult.GradingStartDate.HasValue && mockTestResult.GradingStartDate.Value.AddMinutes(30) < DateTime.Now)
            {
                mockTestResult.GradingTeacherId = null;
                mockTestResult.GradingStartDate = null;
            }
            else
            {
                mockTestResult.GradingTeacherId = teacherId;
                mockTestResult.GradingStartDate = DateTime.Now;
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
                .Select(x => new MockTestResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    SkillScores = x.SkillScores != null ? x.SkillScores : null,
                    Scores = x.SkillScores != null ? x.SkillScores.Average(x => x.Scores) : 0,
                    Percent = x.Percent,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    MockTestId = x.MockTestId,
                    StudentId = x.StudentId,
                    CourseId = x.CourseId,
                    UnitId = x.UnitId,
                    GradingStartDate = x.GradingStartDate,
                    GradingTeacherId = x.GradingTeacherId,
                }).FirstOrDefault()
            };
            mockTestResult = _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = mockTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
