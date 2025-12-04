// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Fsel.System.Application.Services.UserServices.Models;
    using Fsel.System.Application.Services.UserServices.Models.QueryModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/v1/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([FromRoute] Guid id);

        [Put("/v1/student/update-student-class")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByClassAsync([Body] UpdateStudentByClassIdModel command);

        [Put("/v1/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/student/get-class-has-too-many-students/{id}")]
        Task<IApiResponse<MethodResult<bool>>> GetStudentByClassIdCheckAsync([FromRoute] Guid id);

        [Put("/v1/student/delete-student-from-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteStudentFromClass([FromRoute] Guid id);

        [Post("/v1/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [Get("/v1/teacher/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([FromRoute] Guid id);

        [Post("/v1/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Post("/v1/admin/cso")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetCSOByIds([Body] IList<Guid>? ids);

        [Get("/v1/cso/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCsoByUserIdAsync([FromRoute] Guid id);

        [Get("/v1/admin/cso/get-all")]
        Task<IApiResponse<MethodResult<IList<CSOModel>>>> GetAllCSO();

        [Get("/v1/teacher/get-all")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetAllTeacher();

        [Get("/v1/teacher/get-teachers-by-keyword/{keyword}")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeachersByKeyword([FromRoute] string? keyword);

        [Post("/v1/platform/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<PlatformModel>>>> GetPlatformsQueryAsync([Body] BaseQueryModel query);

        [Get("/v1/user/get-users-by-role")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUserByRoleAsync([Query] GetUsersByRoleQueryModel query);

        [Post("/v1/student/get-student-by-emails")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByEmails([Body] IList<string> emails);

        [Get("/v1/student-ranking/check-lucky-spin")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>?>>> CheckLuckySpin([Query] Guid? userId);

        [Post("/v1/student-focus-time")]
        Task<IApiResponse<MethodResult<IList<StudentFocusTimeModel>>>> SaveFocusTime([Body] StudentFocusTimeCommandModel cmd);

        [Post("/v1/student-daily-streak")]
        Task<IApiResponse<MethodResult<bool>>> SaveDailyStreak([Body] StudentDailyStreakCommandModel cmd);

        [Get("/v1/student-ranking/get-events-by-user-id")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>?>>> GetEventsByUserId([Query] Guid? userId);

        [Post("/v1/user/get-users-by-userids")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUsersByUserIdsAsync([FromBody] IList<Guid>? userIds);

        [Get("/v1/user/get-user-profile")]
        Task<IApiResponse<MethodResult<UserModel>>> GetUserProfileAsync();

        [Get("/v1/admin/student/search")]
        Task<IApiResponse<MethodResult<PagingItemsModel<StudentDtoModel>>>> SearchStudentSchoolAsync([FromQuery] SearchStudentSchoolQueryModel query);

        [RefitCache(CacheSettings.TimeCache.TenMinutes)]
        [Get("/v1/admin/student/gets")]
        Task<IApiResponse<MethodResult<IList<StudentDtoModel>>>> GetStudentsSchoolAsync([FromQuery] SearchStudentSchoolQueryModel query);

        [Post("/v1/event/event-ids")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventByIds([FromBody] GetEventByIdsModel query);

        [Post("/v1/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByUserIds([Body] IList<Guid> ids);

        [Get("/v1/admin-school/student/schoolId")]
        [RefitCache(CacheSettings.TimeCache.OneHour)]
        Task<IApiResponse<MethodResult<Guid>>> GetSchoolIdAsync();

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdWithCacheAsync([FromRoute] Guid id);
    }
}