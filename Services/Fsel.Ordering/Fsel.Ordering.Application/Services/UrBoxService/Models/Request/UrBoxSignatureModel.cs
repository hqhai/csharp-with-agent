// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Fsel.Ordering.Infrastructure.ValueSettings;

    public class UrBoxSignatureModel
    {
        public UrBoxSignatureModel(AppSetting appSetting)
        {
            AppSecret = appSetting?.UrBoxConfig?.AppSecret;
            AppId = appSetting?.UrBoxConfig?.AppId;
        }

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
