// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.SystemService.Model;
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

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Post("/v1/school/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetSchoolByIds([Body] IList<Guid>? ids);

        [Post("/v1/google-sheet/register-student-for-event")]
        Task<IApiResponse<MethodResult<bool>>> RegisterStudentForEvent([Body] RegisterStudentForEventCommandModel model);
    }
}
