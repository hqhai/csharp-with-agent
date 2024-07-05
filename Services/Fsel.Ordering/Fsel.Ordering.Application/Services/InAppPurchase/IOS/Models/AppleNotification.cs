// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Newtonsoft.Json;

    public class AppleNotification
    {
        [JsonProperty("signedPayload")]
        public string SignedPayload { get; set; } = string.Empty;
    }
}
