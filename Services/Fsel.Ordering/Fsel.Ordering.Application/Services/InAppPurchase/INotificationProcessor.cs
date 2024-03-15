// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;

    public interface INotificationProcessor
    {
        Task<bool> Process(AppleNotification notification);

        TransactionInfoV2? TransactionInfo(string signedTransactionInfo);
    }
}
