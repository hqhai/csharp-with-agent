// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums;
    using Newtonsoft.Json;

    public class RenewalInfoV2
    {
        [JsonProperty("autoRenewProductId")]
        public string AutoRenewProductId { get; set; }

        [JsonProperty("autoRenewStatus")]
        public AutoRenewStatus AutoRenewStatus { get; set; }

        [JsonProperty("expirationIntent")]
        public EnumExpirationIntent ExpirationIntent { get; set; }

        [JsonProperty("gracePeriodExpiresDate")]
        public long GracePeriodExpiresDate { get; set; }

        [JsonProperty("isInBillingRetryPeriod")]
        public bool IsInBillingRetryPeriod { get; set; }

        [JsonProperty("offerIdentifier")]
        public string OfferIdentifier { get; set; }

        [JsonProperty("offerType")]
        public EnumOfferType OfferType { get; set; }

        [JsonProperty("originalTransactionId")]
        public string OriginalTransactionId { get; set; }

        [JsonProperty("priceIncreaseStatus")]
        public EnumPriceConsent PriceIncreaseStatus { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("signedDate")]
        public long SignatureDate { get; set; }
    }
}
