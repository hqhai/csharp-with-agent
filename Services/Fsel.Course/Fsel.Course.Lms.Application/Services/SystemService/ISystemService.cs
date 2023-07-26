// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Refit;

    public interface ISystemService
    {
        [Post("/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigAsync([Body] IList<Guid> courseIds);

        
    }
}
