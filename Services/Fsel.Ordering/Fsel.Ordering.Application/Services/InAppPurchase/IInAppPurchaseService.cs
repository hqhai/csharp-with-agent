// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Common.ActionResults;
    using Refit;

    public interface IInAppPurchaseService
    {
        [Post("/inApps/v1/notifications/test")]
        Task<IApiResponse<MethodResult<object>>> GetNotification();
    }
}
