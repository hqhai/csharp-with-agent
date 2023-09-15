// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Domain.Models.EntityModels;
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

        [Post("/feature-access-time")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesByIdsAsync([FromBody] IList<Guid> ids);
    }
}
