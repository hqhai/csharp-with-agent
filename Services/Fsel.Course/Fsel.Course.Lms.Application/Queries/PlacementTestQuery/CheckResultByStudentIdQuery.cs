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

    public class CheckResultByStudentIdQuery : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckResultByStudentIdQueryHandler : IRequestHandler<CheckResultByStudentIdQuery, MethodResult<bool>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public CheckResultByStudentIdQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckResultByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var isCheck = await _placementTestResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == request.StudentId, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = isCheck;
            return methodResult;
        }
    }
}
