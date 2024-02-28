// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchaseService
{
    using Fsel.Ordering.Application.Services.InAppPurchaseService.Models;

    public interface IInAppPurchaseService
    {
        Task<bool> ValidatePurchase(PurchaseDetails purchaseDetails, string? uid, CancellationToken cancellationToken);
    }
}
