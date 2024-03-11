// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IInAppPurchaseService
    {
        [Post("/inApps/v1/notifications/test")]
        Task<IApiResponse<NotificationTokenModel>> GetNotification([Header("Authorization")] string authorizationHeader);

        [Get("/inApps/v1/notifications/test/{token}")]
        Task<IApiResponse<AppleNotification>> GetStatusNotification([FromRoute] string? token, [Header("Authorization")] string authorizationHeader);
    }
}
