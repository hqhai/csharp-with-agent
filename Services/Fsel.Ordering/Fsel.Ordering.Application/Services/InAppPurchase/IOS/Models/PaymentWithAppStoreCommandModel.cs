// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models;

    public class PaymentWithAppStoreCommandModel
    {
        public NotificationV2? DecodedPayload { get; set; }
        public RenewalInfoV2? RenewalInfo { get; set; }
        public TransactionInfoV2? TransactionInfo { get; set; }
    }
}
