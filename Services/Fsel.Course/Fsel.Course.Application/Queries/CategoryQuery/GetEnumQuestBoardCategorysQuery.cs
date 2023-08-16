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

    public class GetEnumQuestBoardCategorysQuery : IRequest<MethodResult<object>>
    {
        public EnumQuestBoardType? QuestBoardType { get; set; }
    }

    public class GetEnumQuestBoardCategorysQueryHandler : IRequestHandler<GetEnumQuestBoardCategorysQuery, MethodResult<object>>
    {
        public GetEnumQuestBoardCategorysQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetEnumQuestBoardCategorysQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            methodResult.Result = request.QuestBoardType.GetEnumQuestBoardCategorys();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
