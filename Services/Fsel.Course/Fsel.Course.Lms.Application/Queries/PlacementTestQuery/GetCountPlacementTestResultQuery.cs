// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCountPlacementTestResultQuery : IRequest<MethodResult<int>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetCountPlacementTestResultQueryHandler : IRequestHandler<GetCountPlacementTestResultQuery, MethodResult<int>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetCountPlacementTestResultQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<int>> Handle(GetCountPlacementTestResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<int> methodResult = new MethodResult<int>();
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            methodResult.Result = placementTestResults.Count;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
