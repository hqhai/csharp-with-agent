// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetAllEnumPlacementTestSkillsQuery : IRequest<MethodResult<object>>
    {
    }

    public class GetAllEnumPlacementTestSkillsQueryHandler : IRequestHandler<GetAllEnumPlacementTestSkillsQuery, MethodResult<object>>
    {
        public GetAllEnumPlacementTestSkillsQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetAllEnumPlacementTestSkillsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            methodResult.Result = EnumCourseLevelHelper.GetEnumPlacementTestSkills();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
