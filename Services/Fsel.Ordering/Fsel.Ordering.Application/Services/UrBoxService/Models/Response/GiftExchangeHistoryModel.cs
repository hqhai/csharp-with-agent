// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GiftExchangeHistoryModel : UrBoxModel
    {
        [JsonPropertyName("data")]
        public IList<DataGiftExchangeHistory>? Data { get; set; }

        [JsonPropertyName("totalPage")]
        public long TotalPage { get; set; }
    }

    public partial class DataGiftExchangeHistory
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("linkCart")]
        public string? LinkCart { get; set; }

        [JsonPropertyName("linkCombo")]
        public string? LinkCombo { get; set; }

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("created_timestamp")]
        public string? CreatedTimestamp { get; set; }

        [JsonPropertyName("pay_time")]
        public string? PayTime { get; set; }

        [JsonPropertyName("pay_status")]
        public string? PayStatus { get; set; }

        [JsonPropertyName("pay_status_code")]
        public long PayStatusCode { get; set; }

        [JsonPropertyName("detail")]
        public IList<DetailGiftExchangeHistory>? Detail { get; set; }

        [JsonPropertyName("item_quantity")]
        public long ItemQuantity { get; set; }
    }

    public class DetailGiftExchangeHistory
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("usage_status")]
        public string? UsageStatus { get; set; }

        [JsonPropertyName("usage_status_code")]
        public long UsageStatusCode { get; set; }

        [JsonPropertyName("using_time")]
        public string? UsingTime { get; set; }

        [JsonPropertyName("gift_id")]
        public string? GiftId { get; set; }

        [JsonPropertyName("gift_detail_id")]
        public string? GiftDetailId { get; set; }

        [JsonPropertyName("delivery")]
        public string? Delivery { get; set; }

        [JsonPropertyName("deliveryCode")]
        public long DeliveryCode { get; set; }

        [JsonPropertyName("code_image")]
        public string? CodeImage { get; set; }

        [JsonPropertyName("delivery_required")]
        public string? DeliveryRequired { get; set; }

        [JsonPropertyName("topup")]
        public IList<object>? TopUp { get; set; }

        [JsonPropertyName("gift_title")]
        public string? GiftTitle { get; set; }

        [JsonPropertyName("expired")]
        public string? Expired { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("code_display")]
        public string? CodeDisplay { get; set; }

        [JsonPropertyName("code_display_type")]
        public long CodeDisplayType { get; set; }

        [JsonPropertyName("gift_detail_title")]
        public string? GiftDetailTitle { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("images_rectangle")]
        public string? ImagesRectangle { get; set; }

        [JsonPropertyName("brandTitle")]
        public string? BrandTitle { get; set; }

        [JsonPropertyName("brandImage")]
        public string? BrandImage { get; set; }
    }
}
