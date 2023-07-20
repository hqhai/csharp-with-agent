// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestDetailByTeacherQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestDetailByTeacherQueryHandler : IRequestHandler<GetMockTestDetailByTeacherQuery, MethodResult<MockTestModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;

        public GetMockTestDetailByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, SectionConverter sectionConverter)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestDetailByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultNotExist));
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
                                    .Where(x => x.Id == mockTestResult.MockTestId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);

            var mockTestModel = new MockTestModel
            {
                Id = mockTest!.Id,
                Name = mockTest.Name,
                MockTestType = mockTest.MockTestType,
                CourseType = mockTest.CourseType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any(),
                SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
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
                    UnitId = x.UnitId
                }).FirstOrDefault()
            };

            methodResult.Result = mockTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
