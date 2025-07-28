// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Training.Application.Services.CourseServices.CommandModels;
    using Fsel.Training.Application.Services.CourseServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/v1/cso/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Get("/v1/cso/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/v1/placement-test/admin")]
        Task<IApiResponse<MethodResult<List<AveragePTPointModel>>>> GetAveragePTPoint([FromBody] GetAveragePTPointByIdsQueryModel model);

        [Get("/v1/teacher/course/get-course-by-ids")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCourseByIdsFromTeacherAsync([Query] IList<Guid> ids);

        [Get("/v1/teacher/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelFromTeacherAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/v1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetListCourseByIds([FromBody] IList<Guid> courseIds);

        [Get("/v1/course/execute-query")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByLevel([FromQuery] BaseQueryModel baseQuery);

        [Post("/v1/placement-test-result/choose-level-pt")]
        Task<IApiResponse<MethodResult<bool>>> ChooseLevelPTAsync([FromBody] SavePlacementTestGroupResultCommandModel command);

        [Get("/v1/course/get-course-for-choose-level")]
        Task<IApiResponse<MethodResult<CourseForChooseLevelModel>>> GetCourseForChooseLevel([Query] GetCourseForChooseLevelQueryModel command);
    }
}
