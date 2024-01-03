// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.UrBox
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class UrBoxSignatureModel
    {
        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }

        [JsonPropertyName("app_secret")]
        public string? AppSecret { get; set; }

        [JsonPropertyName("dataBuy")]
        public IList<DataBuy>? DataBuy { get; set; }

        [JsonPropertyName("isSendSms")]
        public int IsSendSms { get; set; }

        [JsonPropertyName("site_user_id")]
        public string? SiteUserId { get; set; }

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }
    }

    public class DataBuy
    {
        [JsonPropertyName("priceId")]
        public string? PriceId { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }
    }
}
