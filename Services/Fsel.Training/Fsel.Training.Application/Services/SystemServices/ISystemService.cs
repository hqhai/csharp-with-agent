// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices.Model;
    using Refit;

    public interface ISystemService
    {
        [Post("/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigByCourseId([Body] IList<Guid> courseIds);
    }
}
