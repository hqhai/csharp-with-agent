// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models;

    public interface INotificationProcessor
    {
        Task<bool> Process(AppleNotification notification);

        TransactionInfoV2? TransactionInfo(string signedTransactionInfo);
    }
}
