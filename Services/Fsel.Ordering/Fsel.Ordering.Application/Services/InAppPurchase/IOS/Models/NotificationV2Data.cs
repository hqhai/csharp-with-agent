// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums;
    using Newtonsoft.Json;

    public class NotificationV2Data
    {
        [JsonProperty("appAppleId")]
        public string AppAppleId { get; set; }

        [JsonProperty("bundleId")]
        public string BundleId { get; set; }

        [JsonProperty("bundleVersion")]
        public string BundleVersion { get; set; }

        [JsonProperty("environment")]
        public EnumAppStoreEnvironment Environment { get; set; }

        [JsonProperty("signedRenewalInfo")]
        public string SignedRenewalInfo { get; set; }

        [JsonProperty("signedTransactionInfo")]
        public string SignedTransactionInfo { get; set; }
    }
}
