// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetAllEnumCourseSkillQuery : IRequest<MethodResult<object>>
    {
    }

    public class GetAllEnumCourseSkillQueryHandler : IRequestHandler<GetAllEnumCourseSkillQuery, MethodResult<object>>
    {
        public GetAllEnumCourseSkillQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetAllEnumCourseSkillQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            methodResult.Result = EnumCourseLevelHelper.GetEnumPlacementTestSkills();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
