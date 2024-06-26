// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
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
