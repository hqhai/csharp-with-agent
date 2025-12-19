// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.SystemService.CommandModels;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.Identity.Domain.Models.CommandModels.LandingPages;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;

    //using Fsel.Identity.Application.Services.SystemService.Model;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/focus-time-config")]
        Task<IApiResponse<MethodResult<IList<FocusTimeConfigModel>>>> GetFocusTimeConfig();

        [Get("/v1/token-config/get-token")]
        Task<IApiResponse<MethodResult<TokenConfigModel>>> GetTokenConfigAsync([Query] GetTokenQueryModel query);

        [Delete("/v1/admin/student/delete-student/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteListDataUser([FromRoute] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/school/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> ExecuteListSchoolQueryAsync([Query] BaseQueryModel query);

        [Post("/v1/feature-access-time/get-feature-access-time-by-userIds")]
        Task<IApiResponse<MethodResult<IList<GetFeatureAccessTimeQueryModel>>>> GetFeatureAccessTimeByUserIds([FromBody] IList<Guid> userIds);

        [Post("/v1/google-sheet/add-contact-info-from-landing-page")]
        Task<IApiResponse<MethodResult<bool>>> AddContactInfoToGoogleSheet([Body] ReceiveDataFromLandingPageCommandModel model);

        [Post("/v1/school/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetSchoolByIds([Body] IList<Guid>? ids);

        [Post("/v1/google-sheet/register-student-for-event")]
        Task<IApiResponse<MethodResult<bool>>> RegisterStudentForEvent([Body] RegisterStudentForEventCommandModel model);

        [Get("/v1/location/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetLocationByIdsAsync([Query] GetLocationsByIdsQueryModel query);

        [Post("/v1/school/gets")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetSchoolsAsync([Body] GetListSchoolQueryModel query);

        [Post("/v1/school/get-ids")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetSchoolIdsAsync([Body] GetSchoolsQueryModel query);

        [Post("/v1/admin/token-history/survey-reward")]
        Task<IApiResponse<MethodResult<bool>>> AddCoinSurveyReward([Body] AddCoinSurveyRewardModel query);

        [Get("/v1/admin/blind-box")]
        Task<IApiResponse<MethodResult<BlindBoxModel>>> GetBlindBoxAsync();

        [Post("/v1/admin/blind-box/create-multiple")]
        Task<IApiResponse<MethodResult<int>>> CreateBlindBoxesAsync([Body] CreateBlindBoxesCommandModel command);

        [Post("/v1/admin/blind-box/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetBlindBoxesByUserIdsAsync([Body] GetBlindBoxesByUserIdsQueryModel query);

        [Get("/v1/location/{localId}")]
        Task<IApiResponse<MethodResult<SchoolModel>>> GetLocationByLocalId([FromRoute] string localId);

        [Post("/v1/admin/token-history/add-coin-buy-course")]
        Task<IApiResponse<MethodResult<bool>>> AddCoinBuyCourse([Body] AddCoinBuyCourseModel query);

        [Post("/v1/manager-report/admin/aggregate-data-students-in-event")]
        Task<IApiResponse<MethodResult<IList<AggregateDataOtherStudentsInEventModel>>>> AggregateDataStudentsInEvent([FromBody] AggregateDataOtherStudentsInEventQueryModel students);

        [Post("/v1/admin/token-history/add-coin-fsel-event-reward")]
        Task<IApiResponse<MethodResult<bool>>> AddCoinFselEventReward([Body] AddCoinFselEventRewardModel query);

        [Post("/v1/google-sheet/add-dynamic-info-to-google-sheet-file")]
        Task<IApiResponse<MethodResult<bool>>> AddDynamicInfoToGoogleSheetFile([Body] CreateDynamicInfosToGoogleSheetFileCommandModel model);

        [Post("/v1/token-history/create-history-deduct-coin-of-student")]
        Task<IApiResponse<MethodResult<bool>>> CreateHistoryDeductCoinOfStudent([Body] CreateHistoryDeductCoinOfStudentCommandModel query);

        [Post("/v1/course-suggest-config/level-suggestion-users")]
        Task<IApiResponse<MethodResult<IList<CourseSuggestUsersModel>>>> GetCourseSuggestByUserIds([Body] GetCourseSuggestByUserIdsQueryModel query);

        [Get("/v1/location/get-by-global-id/{id}")]
        Task<IApiResponse<MethodResult<SchoolModel>>> GetLocationByGlobalId([FromRoute] string id);

        [Get("/v1/sender-config")]
        Task<IApiResponse<MethodResult<IList<SenderConfigModel>>>> GetSenderConfigs();
    }
}
