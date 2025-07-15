// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.CategoryQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetExamPracticeSubTypesQuery : IRequest<MethodResult<object>>
    {
        public EnumExamPracticeType Type { get; set; }
    }

    public class GetExamPracticeSubTypesQueryHandler : IRequestHandler<GetExamPracticeSubTypesQuery, MethodResult<object>>
    {
        public GetExamPracticeSubTypesQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetExamPracticeSubTypesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            methodResult.Result = request.Type.GetExamPracticeSubTypes();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
