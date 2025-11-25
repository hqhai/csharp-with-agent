// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Constants;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Post("/v1/course-time-config/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> CourseTimeConfigQueryAsync([Body] BaseQueryModel query);

        [Get("/v1/course-time-config")]
        Task<IApiResponse<MethodResult<PagingItemsModel<CourseTimeConfigModel>>>> GetCourseTimeConfigAsync();

        [Get("/v1/log-action/{userId}")]
        Task<IApiResponse<MethodResult<LogActionDaysModel>>> GetLogActionsByUserId([FromRoute] Guid userId);

        [Post("/v1/log-action")]
        Task<IApiResponse<MethodResult<IList<LogActionDaysModel>>>> GetLogActionsByUserIdsAsync([FromBody] IList<Guid> ids);

        [Post("/v1/feature-access-time/gets")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesAsync([FromBody] FeatureAccessTimesQueryModel query);

        [Post("/v1/feature-access-time/get-to-modules")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimeToModulesAsync([FromBody] FeatureAccessTimesQueryModel query);

        [RefitCache(CacheSettings.TimeCache.FiveMinutes)]
        [Get("/v1/feature-access-time/get-detail")]
        Task<IApiResponse<MethodResult<FeatureAccessTimeModel>>> GetFeatureAccessTimeAsync([FromQuery] FeatureAccessTimeQueryModel query);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<string>>>> CheckContainForbiddenWord([FromQuery] string Word);

        [RefitCache(CacheSettings.TimeCache.TenMinutes)]
        [Get("/v1/token-config/get-token")]
        Task<IApiResponse<MethodResult<TokenConfigModel>>> GetTokenConfigAsync([Query] GetTokenQueryModel query);

        [RefitCache(CacheSettings.TimeCache.TenMinutes)]
        [Get("/v1/token-config/get-tokens")]
        Task<IApiResponse<MethodResult<IList<TokenConfigModel>>>> GetTokenConfigsAsync([Query] GetTokenConfigsQueryModel query);

        [Get("/v1/feature-access-time/get-feature-access-time-business")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeBusinessModel>>>> GetFeatureAccessTimeBusiness([FromQuery] GetFeatureAccessTimeBusinessQueryModel model);

        [Get("/v1/feature-access-time/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetListFeatureAccessTime([FromQuery] BaseQueryModel model);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Post("/v1/school/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetSchoolsAsync([Body] IList<Guid>? ids);

        [Post("/v1/feature-access-time/get-feature-access-time-by-userIds")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimeByUserIdsAsync([FromBody] IList<Guid> userIds);

        [Post("/v1/feature-access-time/feature-access/time-range")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimeRangeByUserIds([FromBody] GetFeatureAccessTimesByUserIdsQueryModel query);

        [Post("/v1/google-sheet/add-error-report-explanation-question")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> AddErrorReportExplanationQuestionToGoogleSheet([FromBody] AddErrorReportExplanationQuestionModel model);

        [Post("/v1/feature-access-time/get-feature-access-time-to-modules")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesAsync([FromBody] GetFeatureAccessTimeToExportQueryModel query);

        [Post("/v1/school/get-ids")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetSchoolIdsAsync([Body] GetSchoolsQueryModel query);

        [Delete("/v1/course-target-student/{studentId}")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> DeleteCourseTargetStudent([FromRoute] Guid studentId);

        [Post("/v1/feature-access-time/get-feature-accesstime")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetAccessTimeByUserAndFeature([Body] GetAccessTimeByUserAndFeatureQueryModel query);

        [Post("/v1/course-suggest-config/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<CourseSuggestConfigModel>>>> CourseSuggestConfigQuery([Body] BaseQueryModel model);

        [Post("/v1/chat-bot-config/unit-chatbot-configs")]
        Task<IApiResponse<MethodResult<IList<ChatbotConfigModel>>>> GetUnitChatbotConfigsByUnitIds([Body] GetUnitChatbotConfigsQueryModel query);

        [Post("/v1/feature-access-time/get-feature-access-time-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetListFeatureAccessTimeByUserIds([Body] GetFeatureAccessTimesByUserIdsQueryModel model);

        [Post("/v1/feature-access-time/get-last-feature-access-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetLastFeatureAccessByUserIdsAsync([Body] GetFeatureAccessTimesByUserIdsQueryModel model);

        [Get("/v1/admin/course-goal/gets")]
        Task<IApiResponse<MethodResult<IList<CourseGoalModel>>>> GetListCourseGoalAsync();
    }
}
