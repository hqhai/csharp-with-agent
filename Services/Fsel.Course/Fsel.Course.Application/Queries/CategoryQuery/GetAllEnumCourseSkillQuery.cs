// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetAllEnumCourseSkillQuery : IRequest<MethodResult<object>>
    {
        public EnumPlacementTestLevel? PlacementTestLevel { get; set; }
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

            methodResult.Result = EnumCourseLevelHelper.GetEnumCourseSkills();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
