// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetEnumReviewTypeQuery : IRequest<MethodResult<object>>
    {
        public EnumReviewType? ReviewType { get; set; }
    }

    public class GetEnumReviewTypeQueryHandler : IRequestHandler<GetEnumReviewTypeQuery, MethodResult<object>>
    {
        public GetEnumReviewTypeQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetEnumReviewTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            methodResult.Result = request.ReviewType.GetEnumReviewTypes();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
