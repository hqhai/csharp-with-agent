// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.CourseServices.Models;
    using global::System;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/cso/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Get("/cso/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/placement-test/admin")]
        Task<IApiResponse<MethodResult<List<AveragePTPointModel>>>> GetAveragePTPoint([FromBody] GetAveragePTPointByIdsQueryModel model);

        [Get("/teacher/course/get-course-by-ids")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCourseByIdsFromTeacherAsync([Query] IList<Guid> ids);

        [Get("/teacher/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelFromTeacherAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetListCourseByIds([FromBody] IList<Guid> courseIds);

        [Get("/quest-board/percent-video")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentVideoResult([Query] GetFinishOneLessonQueryModel query);

        [Get("/quest-board/percent-unit")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentUnitResult([Query] GetFinishOneQueryModel query);

        [Get("/quest-board/percent-course")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentCourseResult([Query] GetFinishOneQueryModel query);
    }
}
