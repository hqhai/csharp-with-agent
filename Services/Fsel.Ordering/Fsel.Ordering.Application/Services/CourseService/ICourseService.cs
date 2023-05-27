// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.CourseService
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.CourseService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);
    }
}
