// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetMockTestResultReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestResultReportQueryHandler : IRequestHandler<GetMockTestResultReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestResultReportModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<TestResultReportModel>(mockTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
