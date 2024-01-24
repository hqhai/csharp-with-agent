// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    using System.Text.Json.Serialization;

    public class PayooModel
    {
        [JsonPropertyName("result")]
        public string? Result { get; set; }
        [JsonPropertyName("order")]
        public OrderPayooModel? Order { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class OrderPayooModel
    {
        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }
        [JsonPropertyName("order_no")]
        public string? OrderNo { get; set; }
        [JsonPropertyName("amount")]
        public string? Amount { get; set; }
        [JsonPropertyName("payment_code")]
        public string? PaymentCode { get; set; }
        [JsonPropertyName("expiry_date")]
        public string? ExpiryDate { get; set; }
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        [JsonPropertyName("payment_url")]
        public string? PaymentUrl { get; set; }
    }
}
