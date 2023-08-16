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

    public class GetEnumReviewQuestionTypesQuery : IRequest<MethodResult<object>>
    {
        public EnumReviewType? ReviewType { get; set; }
    }

    public class GetEnumReviewQuestionTypesQueryHandler : IRequestHandler<GetEnumReviewQuestionTypesQuery, MethodResult<object>>
    {
        public GetEnumReviewQuestionTypesQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetEnumReviewQuestionTypesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            methodResult.Result = request.ReviewType.GetEnumReviewQuestionTypes();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
