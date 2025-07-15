// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Android
{
    using System.Threading.Tasks;
    using Google.Apis.AndroidPublisher.v3.Data;

    public interface IGooglePlayBillingService
    {
        Task<SubscriptionPurchaseV2?> VerifySubscriptionAsync(string packageName, string token);

        Task<ProductPurchase> VerifyProductAsync(string packageName, string productId, string token);
    }
}
