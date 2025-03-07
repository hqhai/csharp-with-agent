// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models;

    public class AppStoreResponseModel
    {
        public EnumNotificationType? NotificationType { get; set; }
        public EnumNotificationSubtype? Subtype { get; set; }
        public string? NotificationUUID { get; set; }
        public string? NotificationVersion { get; set; }
        public string? SignedDate { get; set; }
        public TransactionInfoV2? TransactionInfo { get; set; }
        public RenewalInfoV2? RenewalInfoV2 { get; set; }
    }
}
