// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestResultReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestResultReportQueryHandler : IRequestHandler<GetMockTestResultReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly DateTimeConverter _dateTimeConverter;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMapper mapper, DateTimeConverter dateTimeConverter)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
            _dateTimeConverter = dateTimeConverter;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestResultReportModel>();

            var mockTestResult = await _mockTestResultRepository.Queryable
                            .Include(x => x.SectionGroupResults)
                            .ThenInclude(x => x!.SectionGroup)
                            .Where(x => x.Id == request.MockTestResultId)
                            .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var mockTestResultDto = _mapper.Map<TestResultReportModel>(mockTestResult);

            if (mockTestResultDto != null)
            {
                mockTestResultDto.WorkingTime = _dateTimeConverter.GetWorkingTime(mockTestResult.CreatedDate, mockTestResult.UpdatedDate ?? DateTime.UtcNow, mockTestResult!.SectionGroupResults.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault());
                mockTestResultDto.Score = mockTestResult.SkillScores?.Average(x => x.Scores);
            }

            methodResult.Result = mockTestResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
