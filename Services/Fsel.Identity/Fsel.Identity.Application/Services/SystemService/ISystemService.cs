// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.Models.CommandModels.LandingPages;
    using Fsel.Shared.Models.ShareModels;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/focus-time-config")]
        Task<IApiResponse<MethodResult<IList<FocusTimeConfigModel>>>> GetFocusTimeConfig();

        [Get("/v1/token-config/get-token")]
        Task<IApiResponse<MethodResult<TokenConfigModel>>> GetTokenConfigAsync([Query] GetTokenQueryModel query);

        [Post("/v1/school/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> ExecuteListSchoolQueryAsync([Body] BaseQueryModel query);

        [Post("/v1/google-sheet/add-contact-info-from-landing-page")]
        Task<IApiResponse<MethodResult<bool>>> AddContactInfoToGoogleSheet([Body] ReceiveDataFromLandingPageCommandModel model);
    }
}
