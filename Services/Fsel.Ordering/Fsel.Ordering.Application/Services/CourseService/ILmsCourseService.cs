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
        [Post("/v1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByIdsAsync([FromBody] IList<Guid> courseIds);

        [Get("/v1/course/execute-query")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByLevel([FromQuery] BaseQueryModel baseQuery);

        [Put("/v1/unit-result/open-next-unit/{userId}")]
        Task<IApiResponse<MethodResult<bool>>> UpdateNextUnit([FromRoute] Guid userId);

        [Get("/v1/admin/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);
        [Post("/v1/integration/integration-course")]
        Task<IApiResponse<MethodResult<IList<CourseResultModel>>>> GetCourseResultsByUserIds([FromBody] IList<Guid> userIds);
    }
}
