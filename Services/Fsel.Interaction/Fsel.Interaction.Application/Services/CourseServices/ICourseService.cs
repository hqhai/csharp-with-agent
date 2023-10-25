// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ICourseService
    {
        [Get("/cso/course/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetCourseByIdAsync([FromRoute] Guid id);

        [Get("/cso/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Get("/teacher/course/get-course-by-ids")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCourseByIdsFromTeacherAsync([Query] IList<Guid> ids);

        [Get("/teacher/course/get-course-by-level")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetCoursesByLevelFromTeacherAsync([FromQuery] EnumCourseLevel? courseLevel);

        [Post("/admin/course")]
        Task<IApiResponse<MethodResult<IList<CourseModel>>>> GetListCourseByIds([FromBody] IList<Guid> courseIds);

        [Get("/class-forum-result/{id}")]
        Task<IApiResponse<MethodResult<ClassForumResultModel>>> GetClassForumResultByIdAsync([FromRoute] Guid id);

        [Post("/class-forum-result/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<ClassForumResultModel>>>> ExecuteListClassForumResultQueryAsync([Body] BaseQueryModel query);
    }
}
