// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    public class GetAllEnumCourseLevelQuery : IRequest<MethodResult<object>>
    {
        public EnumCourseType? CourseType { get; set; }
    }

    public class GetAllEnumCourseLevelQueryHandler : IRequestHandler<GetAllEnumCourseLevelQuery, MethodResult<object>>
    {
        public GetAllEnumCourseLevelQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetAllEnumCourseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            methodResult.Result = EnumCourseLevelHelper.GetEnumCourseLevels();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
