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
    using Fsel.Course.Infrastructure.Common;
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
        private readonly DateTimeConverter _dateTimeConverter;

        public GetFinalTestResultReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, DateTimeConverter dateTimeConverter)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _dateTimeConverter = dateTimeConverter;
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
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            var finalTestResultDto = _mapper.Map<TestResultReportModel>(finalTestResult);
            if (finalTestResultDto != null)
            {
                finalTestResultDto.WorkingTime = _dateTimeConverter.GetWorkingTime(finalTestResult.CreatedDate, finalTestResult.UpdatedDate ?? DateTime.UtcNow, finalTestResult.SectionGroupResults.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault());
                finalTestResultDto.Score = finalTestResult.CorrectCount;
            }

            methodResult.Result = finalTestResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
