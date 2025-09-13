// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/v1/student/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> ExecuteListQueryAsync([Query] BaseQueryModel query);

        [Put("/v1/student/update-student-token")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateStudentByTokenAsync([Body] UpdateStudentByTokenModel command);

        [Post("/v1/student/execute-query")]
        Task<IApiResponse<MethodResult<StudentModel>>> ExecuteQueryAsync([Body] BaseQueryModel query);

        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdWithCacheAsync([FromRoute] Guid id);

        [Get("/v1/student/get-student-by-class-id/{id}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByClassIdAsync([Body] Guid id);

        [Get("/v1/student/get-class-has-too-many-students/{id}")]
        Task<IApiResponse<MethodResult<bool>>> GetStudentByClassIdCheckAsync([FromRoute] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Post("/v1/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/teacher/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByIdAsync([Body] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/teacher/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<TeacherModel>>> GetTeacherByUserIdAsync([FromRoute] Guid id);

        [Put("/v1/student/update-student-level")]
        Task<IApiResponse<MethodResult<bool>>> UpdateStudentByLevelAsync([Body] UpdateStudentByLevelModel command);

        [Post("/v1/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid>? studentIds);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/cso/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCSOByUserId([FromRoute] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/cso/get-by-id/{id}")]
        Task<IApiResponse<MethodResult<CSOModel>>> GetCSOById([FromRoute] Guid id);

        [Get("/v1/student-daily-streak/get-daily-streak/{id}")]
        Task<IApiResponse<MethodResult<DailyStreakModel>>> GetDailyStreak([FromRoute] Guid id);

        [Get("/v1/student-focus-time/check-super-fire")]
        Task<IApiResponse<MethodResult<bool>>> CheckSuperFireModeAsync();

        [Get("/v1/student-daily-streak/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentConsecutiveDayModel>>>> GetAllDailyStreak([Query] BaseQueryModel baseQuery);

        [Get("/v1/student-trial-registration/{userId}")]
        Task<IApiResponse<MethodResult<StudentTrialRegistrationModel>>> GetStudentTrialRegistration([FromRoute] Guid userId);

        [Get("/v1/student-trial-registration/check/{id}")]
        Task<IApiResponse<MethodResult<bool>>> CheckStudentTrialRegistration([FromRoute] Guid id);

        [Put("/v1/student-trial-registration")]
        Task<IApiResponse<MethodResult<StudentTrialRegistrationModel>>> UpdateTrialRegistrationStatusAsync([Body] StudentTrialRegistrationModel cmd);

        [Get("/v1/student/get-student-by-email")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByEmailAsync([FromQuery] string email);

        [Post("/v1/student/get-student-by-emails")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByEmailsAsync([FromBody] IList<string> emails);

        [Put("/v1/student/update-course-to-student/{courseId}")]
        Task<IApiResponse<MethodResult<StudentModel>>> UpdateCourseToStudentAsync([FromRoute] Guid courseId);

        [Post("/v1/student/get-student-by-full-names")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByFullNamesAsync([FromBody] IList<string> fullNames);

        [Get("/v1/admin/user/token/{id}")]
        Task<IApiResponse<MethodResult<TokenModel>>> GetJWTAsync([FromRoute] Guid id);

        [Post("/v1/user-setting/users")]
        Task<IApiResponse<MethodResult<List<UserSettingModel>>>> GetListUserSetting([Body] UserSettingQuery query);

        [Get("/v1/user-course-setting/{userId}")]
        Task<IApiResponse<MethodResult<IList<UserCourseSettingModel>>>> GetUserCourseSettingsAsync([FromRoute] Guid userId);

        [Post("/v1/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetUserByIds([FromBody] IList<Guid>? userIds);

        [Get("/v1/user/get-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetUserByStudentId([FromRoute] Guid id);

        [Put("/v1/user/update-code-student")]
        Task<IApiResponse<MethodResult<UserModel>>> UpdateCodeStudentAsync([FromBody] UpdateCodeStudentCommandModel command);

        [Post("/v1/user/get-users-by-userids")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUsersByUserIdsAsync([FromBody] IList<Guid>? userIds);

        [Get("/v1/student-daily-streak/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentConsecutiveDayModel>>>> StudentDailyStreakExecuteQuery([Query] BaseQueryModel baseQuery);

        [Get("/v1/student-ranking/get-events-by-user-id")]
        Task<IApiResponse<MethodResult<IList<CompetitionEventsModel>>>> GetEventByUserId([Query] Guid? userId);

        [Get("/v1/student-ranking/school-event")]
        Task<IApiResponse<MethodResult<PagingItemStudentRankingModel>>> GetLeaderBoardDataAsync([FromQuery] GetStudentCompetitionByEventCodeQueryModel query);

        [Get("/v1/admin/student/management")]
        Task<IApiResponse<MethodResult<PagingItemsModel<StudentSearchAdminModel>>>> SearchStudentAsync([FromQuery] SearchStudentsQueryModel query);

        [Post("/v1/admin/student/search")]
        Task<IApiResponse<MethodResult<PagingItemsModel<StudentDtoModel>>>> SearchStudentSchoolAsync([FromBody] SearchStudentSchoolQueryModel query);

        [RefitCache(CacheSettings.TimeCache.TenMinutes)]
        [Get("/v1/admin/student/gets")]
        Task<IApiResponse<MethodResult<IList<StudentDtoModel>>>> GetStudentsSchoolAsync([FromQuery] SearchStudentSchoolQueryModel query);

        [Get("/v1/admin/student/get-dashboards")]
        [RefitCache(CacheSettings.TimeCache.ThirtyMinutes)]
        Task<IApiResponse<MethodResult<IList<StudentDtoModel>>>> GetStudentsDashboardAsync([FromQuery] GetStudentsDashboardQueryModel query);

        [Get("/v1/admin-school/student")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsToAdminSchoolAsync();

        [Get("/v1/student/get-by-school-id")]
        [RefitCache(CacheSettings.TimeCache.ThirtyMinutes)]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsBySchoolId();

        [Get("/v1/user/get-user-profile")]
        Task<IApiResponse<MethodResult<UserModel>>> GetUserProfileAsync();

        [Get("/v1/student-ranking/get-students-by-event-code")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByEventCode([Query] string eventCode);

        [Get("/v1/admin/other/report-competition-event")]
        Task<IApiResponse<MethodResult<IList<ReportCompetitionEventModel>>>> GetReportCompetitionEventAsync([Query] GetReportCompetitionEventQueryModel query);

        [Get("/v1/admin/other/report-competition-event-school")]
        Task<IApiResponse<MethodResult<IList<ReportCompetitionEventModel>>>> GetReportCompetitionEventSchoolAsync([Query] GetReportCompetitionEventQueryModel query);

        [Get("/v1/admin/other/report-competition-event-district-school")]
        Task<IApiResponse<MethodResult<IList<ReportCompetitionEventModel>>>> GetReportCompetitionEventDistrictSchoolAsync([Query] GetCompetitionEventToEventParentQueryModel query);

        [Get("/v1/student/get-student/{eventCode}")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByEventCode([FromRoute] string eventCode);

        [Get("/v1/admin/other/get-student-event-registrations")]
        Task<IApiResponse<MethodResult<IList<EventRegistrationModel>>>> GetStudentEventRegistrationsAsync([Query] GetReportCompetitionEventQueryModel query);

        [Get("/v1/admin-school/student/schoolId")]
        [RefitCache(CacheSettings.TimeCache.OneHour)]
        Task<IApiResponse<MethodResult<Guid>>> GetSchoolIdAsync();

        [Post("/v1/event/get-student-ids-in-event-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<Guid>?>>> GetStudentsInEventByStudentIds([Body] GetStudentIdsInEventByStudentIdsQueryModel model);

        [Post("/v1/student-export/get-by-user-ids")]
        [RefitCache(CacheSettings.TimeCache.OneHour)]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentExportByIds([FromBody] IList<Guid>? userIds);

        [Post("/v1/campus/update-expired-date-for-students")]
        Task<IApiResponse<MethodResult<bool>>> UpdateExpiredDateForStudentsCampus([Body] UpdateExpiredDateForStudentsCampusCommandModels model);

        [Post("/v1/campus/search-students-by-student-ids")]
        Task<IApiResponse<MethodResult<PagingItemsModel<StudentCampusModel>>>> SearchStudentsByStudentIds([Body] SearchStudentsCampusByStudentIdsQueryModel model);
    }
}
