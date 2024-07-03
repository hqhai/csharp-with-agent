// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums;
    using Newtonsoft.Json;

    public class NotificationV2
    {
        [JsonProperty("notificationType")]
        public EnumNotificationType NotificationType { get; set; }

        [JsonProperty("subtype")]
        public EnumNotificationSubtype Subtype { get; set; }

        [JsonProperty("notificationUUID")]
        public string? NotificationUUID { get; set; }

        [JsonProperty("notificationVersion")]
        public string? NotificationVersion { get; set; }

        [JsonProperty("signedDate")]
        public string? SignedDate { get; set; }

        [JsonProperty("data")]
        public NotificationV2Data? Data { get; set; }
    }
}
