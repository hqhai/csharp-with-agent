// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService.Models;
    using Refit;

    public interface ISystemService
    {
        [Get("/referral-discount-config")]
        Task<IApiResponse<MethodResult<IList<UserReferralResultModel>>>> GetUserReferralAsync();
    }
}
