// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.CourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Services.CourseService.Model;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ILmsCourseService
    {
        [Post("/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByIdsAsync([FromBody] IList<Guid> courseIds);

        [Get("/course/execute-query")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByLevel([FromQuery] BaseQueryModel baseQuery);

        [Put("/unit-result/open-next-unit")]
        Task<IApiResponse<MethodResult<bool>>> UpdateNextUnit();
    }
}
