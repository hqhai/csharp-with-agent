// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestResultReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestResultReportQueryHandler : IRequestHandler<GetFinalTestResultReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IMapper _mapper;

        public GetFinalTestResultReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, IFinalTestAnswerRepository finalTestAnswerRepository, IMapper mapper)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
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
            methodResult.Result = await GetFinalTestReport(finalTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<TestResultReportModel> GetFinalTestReport(FinalTestResult finalTestResult)
        {
            var query = _finalTestAnswerRepository.Queryable.Where(x => x.FinalTestResultId == finalTestResult.Id);
            var finalTestResultDto = _mapper.Map<TestResultReportModel>(finalTestResult);
            finalTestResultDto.TotalQuestion = await query.CountAsync();
            finalTestResultDto.CorrectQuestion = await query.Where(x => x.IsCorrect == true).CountAsync();
            return finalTestResultDto;
        }
    }
}
