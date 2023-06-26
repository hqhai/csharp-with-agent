// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices.Model;
    using Refit;

    public interface ISystemService
    {
        [Post("/course-time-config")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> GetCourseTimeConfigByCourseId([Body] IList<Guid> courseIds);

        [Post("/live-time-frame/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<LiveTimeFrameModel>>>> GetTimeFrameByIdsAsync([Body] GetTimeFrameByIdsModel command);
    }
}
