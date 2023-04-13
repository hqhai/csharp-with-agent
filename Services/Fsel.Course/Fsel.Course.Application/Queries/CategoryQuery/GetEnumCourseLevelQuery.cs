// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetEnumCourseLevelQuery : IRequest<MethodResult<IList<EnumCourseLevel>>>
    {
        public EnumCourseType? CourseType { get; set; }
    }

    public class GetEnumLevelQueryHandler : IRequestHandler<GetEnumCourseLevelQuery, MethodResult<IList<EnumCourseLevel>>>
    {
        public GetEnumLevelQueryHandler()
        {
        }

        public async Task<MethodResult<IList<EnumCourseLevel>>> Handle(GetEnumCourseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumCourseLevel>> methodResult = new MethodResult<IList<EnumCourseLevel>>();

            methodResult.Result = request.CourseType.GetEnumCourseLevels();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
