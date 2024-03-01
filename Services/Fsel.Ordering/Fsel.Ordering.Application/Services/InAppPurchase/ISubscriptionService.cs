// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;

    public interface ISubscriptionService
    {
        void Update(NotificationV2 decodedPayload, RenewalInfoV2? renewalInfo, TransactionInfoV2? transactionInfo);
    }
}
