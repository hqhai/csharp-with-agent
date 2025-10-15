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

        [Post("/v1/admin/unit")]
        Task<IApiResponse<MethodResult<IList<UnitModel>>>> GetListUnitByIds([FromBody] IList<Guid> unitIds);

        [Post("/v1/admin/lesson")]
        Task<IApiResponse<MethodResult<IList<LessonModel>>>> GetListLessonByIds([FromBody] IList<Guid> lessonIds);

        [Get("/v1/quest-board/percent-video")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentVideoResult([Query] GetFinishOneLessonQueryModel query);

        [Get("/v1/quest-board/percent-unit")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentUnitResult([Query] GetFinishOneQueryModel query);

        [Get("/v1/quest-board/percent-course")]
        Task<IApiResponse<MethodResult<QuestBoardCategoryModel>>> GetPercentCourseResult([Query] GetFinishOneQueryModel query);

        [Get("/v1/student/student-setting/{userId}")]
        Task<IApiResponse<MethodResult<StudentSettingModel>>> GetStudentSetting([FromRoute] Guid? userId);

        [Post("/v1.1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByIdsAsync([FromBody] IList<Guid> courseIds);
    }
}
