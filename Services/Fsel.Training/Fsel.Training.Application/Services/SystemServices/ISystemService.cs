// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices.Models;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/live-time-frame")]
        Task<IApiResponse<MethodResult<IList<LiveTimeFrameModel>>>> GetLiveTimeFramesAsync();

        [Post("/v1/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigByCourseId([Body] IList<Guid> courseIds);

        [Get("/v1/course-suggest-config/check-suggestion")]
        Task<IApiResponse<MethodResult<bool>>> CheckCourseSuggetConfigByStudent([Query] CheckCourseSuggetConfigByStudentQueryModel query);
    }
}
