// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Post("/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigAsync([Body] IList<Guid> courseIds);

        [Get("/log-action/{userId}")]
        Task<IApiResponse<MethodResult<LogActionDaysModel>>> GetLogActionsByUserId([FromRoute] Guid userId);

        [Post("/log-action")]
        Task<IApiResponse<MethodResult<IList<LogActionDaysModel>>>> GetLogActionsByUserIdsAsync([FromBody] IList<Guid> ids);

        [Post("/feature-access-time/get-list")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesByIdsAsync([FromBody] IList<Guid> ids, [FromQuery] Guid userId, [FromQuery] Guid courseId, [FromQuery] Guid? unitId, [FromQuery] Guid? lessonId);

        [Post("/feature-access-time/get-list-by-courseIds")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeCourseModel>>>> GetFeatureAccessTimesByCourseIdsAsync([FromBody] IList<Guid> courseIds, [FromQuery] Guid userId);

        [Get("/feature-access-time/get-by-unit")]
        Task<IApiResponse<MethodResult<FeatureAccessTimeCourseModel>>> GetFeatureAccessTimesByUnitIdAsync([FromQuery] GetFeatureAccessTimesByUnitIdQueryModel query);

        [Get("/feature-access-time/get-by-test")]
        Task<IApiResponse<MethodResult<FeatureAccessTimeCourseModel>>> GetFeatureAccessTimesByTestAsync([FromQuery] GetFeatureAccessTimesByTestQueryModel query);
    }
}
