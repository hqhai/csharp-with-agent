// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestResultReportQuery : IRequest<MethodResult<MockTestResultReportModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestResultReportQueryHandler : IRequestHandler<GetMockTestResultReportQuery, MethodResult<MockTestResultReportModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMapper _mapper;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, ICourseRepository courseRepository, IMockTestRepository mockTestRepository, SectionGroupConverter sectionGroupConverter, IMockTestAnswerRepository mockTestAnswerRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _courseRepository = courseRepository;
            _mockTestRepository = mockTestRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestResultReportModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestResultReportModel>();

            var mockTestResult = await _mockTestResultRepository.Queryable
                                            .Include(x => x!.MockTestScores)
                                            .Where(x => x.Id == request.MockTestResultId)
                                            .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            var mockTest = await _mockTestRepository.Queryable.Where(x => x.Id == mockTestResult.MockTestId)
                                            .Include(x => x!.MockTestSections)
                                            .ThenInclude(x => x.SectionGroup)
                                            .FirstOrDefaultAsync(cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            methodResult.Result = await GetMockTestReport(mockTestResult, mockTest);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MockTestResultReportModel> GetMockTestReport(MockTestResult mockTestResult, MockTest mockTest)
        {
            var course = await _courseRepository.GetByIdAsync(mockTestResult.CourseId);
            var mockTestResultDto = _mapper.Map<MockTestResultReportModel>(mockTestResult);
            if (course != null)
            {
                (mockTestResultDto.IsCheckScoreColor, mockTestResultDto.TargetBandScore) = course.CourseLevel.CheckScoreColor(mockTestResultDto.Score ?? default);
            }
            mockTestResultDto.IsTeacherGraded = await _sectionGroupConverter.IsTeacherGraded(mockTestResult, mockTest.MockTestSections.Select(x => x.SectionGroup!.CourseSkill).ToList());
            return mockTestResultDto;
        }
    }
}
