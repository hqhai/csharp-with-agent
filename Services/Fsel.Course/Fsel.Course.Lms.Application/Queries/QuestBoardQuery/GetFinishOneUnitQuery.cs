// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestBoardQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetFinishOneUnitQuery : IRequest<MethodResult<double>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetFinishOneUnitQueryHandler : IRequestHandler<GetFinishOneUnitQuery, MethodResult<double>>
    {
        public GetFinishOneUnitQueryHandler()
        {
        }

        public async Task<MethodResult<double>> Handle(GetFinishOneUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<double> methodResult = new MethodResult<double>();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = 0;
            return methodResult;
        }
    }
}
