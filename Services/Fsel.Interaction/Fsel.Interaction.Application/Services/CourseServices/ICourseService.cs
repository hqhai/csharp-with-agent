// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.CourseServices.QueryModel;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/v1/cso/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Get("/v1/cso/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Get("/v1/teacher/course/get-course-by-ids")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCourseByIdsFromTeacherAsync([Query] IList<Guid> ids);

        [Get("/v1/teacher/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelFromTeacherAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/v1/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetListCourseByIds([FromBody] IList<Guid> courseIds);

        [Get("/v1/class-forum-result/{id}")]
        Task<IApiResponse<MethodResult<ClassForumResultModel>>> GetClassForumResultByIdAsync([FromRoute] Guid id);

        [Post("/v1/class-forum-result/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<ClassForumResultModel>>>> ExecuteListClassForumResultQueryAsync([Body] BaseQueryModel query);

        [Get("/v1/class-forum-result/execute-query")]
        Task<IApiResponse<MethodResult<ClassForumResultInfoModel>>> GetClassForumResultInfoByIdAsync([FromQuery] BaseQueryModel query);

        [Get("/v1/course/get-course-studying")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseStudying();

        [Get("/v1.1/lesson/detail")]
        Task<IApiResponse<MethodResult<LessonModel>>> GetLessonResult([FromRoute] Guid LessonResultId);

        [Get("/v1/other/feature-module")]
        Task<IApiResponse<MethodResult<FeatureModuleModel>>> GetModuleModel([FromQuery] FeatureModuleQuery query);

        [Get("/v1/admin/other/get-placement-test-event")]
        Task<IApiResponse<MethodResult<IList<ReportPlacementTestEventModel>>>> GetReportPlacementTestEventAsync([FromQuery] GetReportPlacementTestEventQueryModel query);

        [Get("/v1/admin/other/get-placement-test-school-event")]
        Task<IApiResponse<MethodResult<IList<ReportPlacementTestEventModel>>>> GetReportPlacementTestEventSchoolAsync([FromQuery] GetReportPlacementTestEventQueryModel query);

        [Post("/v1/admin/course/get-course-ids-by-class-forum-result-ids")]
        Task<IApiResponse<MethodResult<IList<CourseClassForumResultModel>>>> GetCourseIds([Body] GetCourseIdsByClassForumResultIdsModel model);
    }
}
