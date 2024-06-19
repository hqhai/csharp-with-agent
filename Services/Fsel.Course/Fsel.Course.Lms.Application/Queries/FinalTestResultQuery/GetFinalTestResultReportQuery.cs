// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
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

    public class GetFinalTestResultReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestResultReportQueryHandler : IRequestHandler<GetFinalTestResultReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;

        public GetFinalTestResultReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetFinalTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestResultReportModel>();

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<TestResultReportModel>(finalTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
