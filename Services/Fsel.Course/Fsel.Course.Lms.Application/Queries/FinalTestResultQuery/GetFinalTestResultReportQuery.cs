// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
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

            var finalTestResult = await _finalTestResultRepository.Queryable
                            .Include(x => x.SectionGroupResults)
                            .ThenInclude(x => x!.SectionGroup)
                            .Where(x => x.Id == request.FinalTestResultId)
                            .FirstOrDefaultAsync(cancellationToken);

            var finalTestResultDto = _mapper.Map<TestResultReportModel>(finalTestResult);
            if (finalTestResultDto != null)
            {
                finalTestResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(finalTestResult?.CreatedDate, finalTestResult?.UpdatedDate ?? DateTime.UtcNow, finalTestResult.SectionGroupResults.Select(x => x.SectionGroup.ExecutionTime).FirstOrDefault());
            }

            methodResult.Result = finalTestResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
