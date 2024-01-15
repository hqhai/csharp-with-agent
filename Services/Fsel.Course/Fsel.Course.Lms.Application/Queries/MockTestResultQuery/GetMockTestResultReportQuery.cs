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
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMapper _mapper;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, SectionGroupConverter sectionGroupConverter, IMockTestAnswerRepository mockTestAnswerRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestResultReportModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestResultReportModel>();

            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.Id == request.MockTestResultId)
                                            .Include(x => x.MockTest)
                                            .ThenInclude(x => x!.MockTestSections)
                                            .ThenInclude(x => x.SectionGroup)
                                            .ThenInclude(x => x!.MockTestScores.Where(x => x.MockTestResultId == request.MockTestResultId))
                                            .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<MockTestResultReportModel>(mockTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
