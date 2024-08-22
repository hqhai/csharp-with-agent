// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/referral-discount-config")]
        Task<IApiResponse<MethodResult<IList<ReferralDiscountConfigModel>>>> GetReferralDiscountConfigAsync();

        [Post("/v1/google-sheet/add-payment-info-to-google-sheet")]
        Task<IApiResponse<MethodResult<VoidMethodResult>>> AddPaymentInfoToGoogleSheet([Body] AddPaymentInfoToGoogleSheetModel model);
    }
}
