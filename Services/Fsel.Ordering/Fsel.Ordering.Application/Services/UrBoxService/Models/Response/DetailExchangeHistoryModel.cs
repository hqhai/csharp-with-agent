// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using System.Text.Json.Serialization;

    public class DetailExchangeHistoryModel : UrBoxModel
    {
        [JsonPropertyName("data")]
        public DataDetailExchangeHistory? Data { get; set; }
    }

    public partial class DataDetailExchangeHistory
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("linkCart")]
        public string? LinkCart { get; set; }

        [JsonPropertyName("linkCombo")]
        public string? LinkCombo { get; set; }

        [JsonPropertyName("money_ship")]
        public string? MoneyShip { get; set; }

        [JsonPropertyName("money_total")]
        public string? MoneyTotal { get; set; }

        [JsonPropertyName("created_timestamp")]
        public string? CreatedTimestamp { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("pay_time")]
        public string? PayTime { get; set; }

        [JsonPropertyName("pay_status")]
        public string? PayStatus { get; set; }

        [JsonPropertyName("pay_status_code")]
        public long PayStatusCode { get; set; }

        [JsonPropertyName("customer")]
        public bool Customer { get; set; }

        [JsonPropertyName("receiver")]
        public Receiver? Receiver { get; set; }

        [JsonPropertyName("item_quantity")]
        public long ItemQuantity { get; set; }

        [JsonPropertyName("detail")]
        public IList<DetailExchangeHistory>? Detail { get; set; }
    }

    public partial class DetailExchangeHistory
    {
        [JsonPropertyName("using_time")]
        public string? UsingTime { get; set; }

        [JsonPropertyName("finish_time")]
        public string? FinishTime { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("delivery")]
        public string? Delivery { get; set; }

        [JsonPropertyName("deliveryCode")]
        public long DeliveryCode { get; set; }

        [JsonPropertyName("delivery_tracking")]
        public string? DeliveryTracking { get; set; }

        [JsonPropertyName("estimateDelivery")]
        public string? EstimateDelivery { get; set; }

        [JsonPropertyName("delivery_note")]
        public string? DeliveryNote { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("topup")]
        public IList<object>? TopUp { get; set; }

        [JsonPropertyName("gift_id")]
        public string? GiftId { get; set; }

        [JsonPropertyName("priceId")]
        public string? PriceId { get; set; }

        [JsonPropertyName("created_timestamp")]
        public string? CreatedTimestamp { get; set; }

        [JsonPropertyName("expired")]
        public string? Expired { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("code_display")]
        public string? CodeDisplay { get; set; }

        [JsonPropertyName("code_display_type")]
        public long CodeDisplayType { get; set; }
    }

    public partial class Receiver
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
