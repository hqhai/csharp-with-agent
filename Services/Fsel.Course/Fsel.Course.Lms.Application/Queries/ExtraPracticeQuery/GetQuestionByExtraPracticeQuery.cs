// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetQuestionByExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeModel>>
    {
    }

    public class GetQuestionByExtraPracticeQueryHandler : IRequestHandler<GetQuestionByExtraPracticeQuery, MethodResult<ExtraPracticeModel>>
    {
        public GetQuestionByExtraPracticeQueryHandler()
        {
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(GetQuestionByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            methodResult.Result =;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
