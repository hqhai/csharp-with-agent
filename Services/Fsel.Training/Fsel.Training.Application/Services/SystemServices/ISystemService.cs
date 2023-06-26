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
        [Get("/live-time-frame")]
        Task<IApiResponse<MethodResult<IList<LiveTimeFrameModel>>>> GetLiveTimeFramesAsync();

        [Post("/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigByCourseId([Body] IList<Guid> courseIds);
    }
}
