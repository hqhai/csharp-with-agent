// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
    using Newtonsoft.Json;

    public class NotificationTokenModel
    {
        [JsonProperty("testNotificationToken")]
        public string? TestNotificationToken { get; set; }
    }
}
