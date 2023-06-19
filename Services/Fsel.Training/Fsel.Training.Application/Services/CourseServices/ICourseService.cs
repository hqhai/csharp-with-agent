// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/cso/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Get("/cso/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelAsync([FromQuery] EnumCourseLevel? courseLevel);
        [Get("/course/get-by-level")]
        Task<IApiResponse<MethodResult<List<CourseModel>>>> GetCourseByLevel([FromQuery] EnumCourseLevel courseLevel);
    }
}
