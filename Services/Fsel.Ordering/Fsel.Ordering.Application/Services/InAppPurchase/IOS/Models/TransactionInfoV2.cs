// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using System.Text.Json.Serialization;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums;

    public class TransactionInfoV2
    {
        [JsonPropertyName("appAccountToken")]
        public string? AppAccountToken { get; set; }

        [JsonPropertyName("environment")]
        public EnumAppStoreEnvironment? Environment { get; set; }

        [JsonPropertyName("bundleId")]
        public string? BundleId { get; set; }

        [JsonPropertyName("expiresDate")]
        public long ExpiresDate { get; set; }

        [JsonPropertyName("inAppOwnershipType")]
        public EnumOwnershipType InAppOwnershipType { get; set; }

        [JsonPropertyName("isUpgraded")]
        public bool IsUpgraded { get; set; }

        [JsonPropertyName("offerIdentifier")]
        public string? OfferIdentifier { get; set; }

        [JsonPropertyName("offerType")]
        public EnumOfferType OfferType { get; set; }

        [JsonPropertyName("originalPurchaseDate")]
        public long OriginalPurchaseDate { get; set; }

        [JsonPropertyName("originalTransactionId")]
        public long OriginalTransactionId { get; set; }

        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }

        [JsonPropertyName("purchaseDate")]
        public long PurchaseDate { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("revocationDate")]
        public long RevocationDate { get; set; }

        [JsonPropertyName("revocationReason")]
        public CancellationReason RevocationReason { get; set; }

        [JsonPropertyName("subscriptionGroupIdentifier")]
        public string? SubscriptionGroupIdentifier { get; set; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("type")]
        public EnumPurchaseType Type { get; set; }

        [JsonPropertyName("webOrderLineItemId")]
        public string? WebOrderLineItemId { get; set; }

        [JsonPropertyName("signedDate")]
        public long SignatureDate { get; set; }

        [JsonPropertyName("transactionReason")]
        public string? TransactionReason { get; set; }

        [JsonPropertyName("storefront")]
        public string? Storefront { get; set; }

        [JsonPropertyName("storefrontId")]
        public string? StorefrontId { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }
}
