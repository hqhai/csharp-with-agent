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
    using Fsel.Shared.Enums;
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
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMapper _mapper;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, IMockTestAnswerRepository mockTestAnswerRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestResultReportModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestResultReportModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            methodResult.Result = await GetMockTestReport(mockTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MockTestResultReportModel> GetMockTestReport(MockTestResult mockTestResult)
        {
            var query = _mockTestAnswerRepository.Queryable.Where(x => x.MockTestResultId == mockTestResult.Id);
            var mockTestResultDto = _mapper.Map<MockTestResultReportModel>(mockTestResult);
            mockTestResultDto.CorrectQuestion = await query.Where(x => x.IsCorrect == true).CountAsync();
            mockTestResultDto.TotalQuestion = await query.CountAsync();
            mockTestResultDto.IsTeacherGraded = await IsTeacherGraded(mockTestResult);
            return mockTestResultDto;
        }

        private async Task<bool> IsTeacherGraded(MockTestResult mockTestResult)
        {
            var mockTest = await _mockTestRepository.Queryable
                            .Where(x => x.Id == mockTestResult.MockTestId)
                            .Include(x => x.MockTestSections)
                            .ThenInclude(x => x.SectionGroup).ThenInclude(x => x.MockTestScores.Where(x => x.MockTestResultId == mockTestResult.Id))
                            .FirstOrDefaultAsync();
            if (mockTest != null)
            {
                var sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup).Where(x => x.CourseSkill == EnumCourseSkill.Speaking || x.CourseSkill == EnumCourseSkill.Writing);
                if (sectionGroups.Any())
                {
                    return sectionGroups.Any() && sectionGroups.SelectMany(x => x!.MockTestScores).Any();
                }
                return true;
            }
            return false;
        }
    }
}
