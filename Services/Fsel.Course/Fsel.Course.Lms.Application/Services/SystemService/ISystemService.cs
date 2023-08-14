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
    }
}
