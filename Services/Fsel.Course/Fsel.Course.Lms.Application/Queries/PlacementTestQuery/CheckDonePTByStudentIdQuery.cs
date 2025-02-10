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

    public class CheckDonePTByStudentIdQuery : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckDonePTByStudentIdHandler : IRequestHandler<CheckDonePTByStudentIdQuery, MethodResult<bool>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public CheckDonePTByStudentIdHandler(IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckDonePTByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var isCheck = await _placementTestGroupResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == request.StudentId, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = isCheck;
            return methodResult;
        }
    }
}
