// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetEnumCourseSkillQuery : IRequest<MethodResult<IList<EnumCourseSkill>>>
    {
        public EnumPlacementTestLevel? PlacementTestLevel { get; set; }
    }

    public class GetEnumCourseSkillQueryHandler : IRequestHandler<GetEnumCourseSkillQuery, MethodResult<IList<EnumCourseSkill>>>
    {
        public GetEnumCourseSkillQueryHandler()
        {
        }

        public async Task<MethodResult<IList<EnumCourseSkill>>> Handle(GetEnumCourseSkillQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumCourseSkill>> methodResult = new MethodResult<IList<EnumCourseSkill>>();

            methodResult.Result = request.PlacementTestLevel.GetEnumPlacementTestSkills();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
