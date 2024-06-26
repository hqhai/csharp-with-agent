// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IAppStoreService
    {
        [Get("/inApps/v1/transactions/{transactionId}")]
        Task<IApiResponse<SignedTransactionInfoModel>> GetInfoTransaction([Header("Authorization")] string authorizationHeader, [FromRoute] string transactionId);
    }
}
