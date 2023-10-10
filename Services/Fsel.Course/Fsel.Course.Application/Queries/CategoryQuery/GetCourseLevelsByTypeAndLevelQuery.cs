// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetCourseLevelsByTypeAndLevelQuery : IRequest<MethodResult<object>>
    {
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetCourseLevelsByTypeAndLevelHandler : IRequestHandler<GetCourseLevelsByTypeAndLevelQuery, MethodResult<object>>
    {
        public GetCourseLevelsByTypeAndLevelHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetCourseLevelsByTypeAndLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            methodResult.Result = request.CourseType.GetListCourseLevels(request.CourseLevel);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
