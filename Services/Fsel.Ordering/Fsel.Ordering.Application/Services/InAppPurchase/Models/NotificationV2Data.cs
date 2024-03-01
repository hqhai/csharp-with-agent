// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
        public EnvironmentName Environment { get; set; }

        [JsonProperty("signedRenewalInfo")]
        public string SignedRenewalInfo { get; set; }

        [JsonProperty("signedTransactionInfo")]
        public string SignedTransactionInfo { get; set; }
    }
}
