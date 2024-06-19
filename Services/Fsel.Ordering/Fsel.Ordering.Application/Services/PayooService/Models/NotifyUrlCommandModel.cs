// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class NotifyUrlCommandModel
    {
        public string? ResponseData { get; set; }
        public string? SecureHash { get; set; }
        public string? OrderNo { get; set; }
        public decimal OrderCash { get; set; }
        public int PaymentStatus { get; set; }
        public int PaymentMethod { get; set; }
        public string? PurchaseDate { get; set; }
        public string? PurchaseUserName { get; set; }
        public long? ShopId { get; set; }
        public string? BankName { get; set; }
        public string? CardNumber { get; set; }
        public string? BillingCode { get; set; }
        public string? CardIssuanceType { get; set; }
        [JsonProperty("Customer_identifier")]
        [JsonPropertyName("Customer_identifier")]
        public string? CustomerIdentifier { get; set; }
        public string? MDD1 { get; set; }
        public string? MDD2 { get; set; }
        public string? Token { get; set; }
        public decimal? VoucherTotalAmount { get; set; }
        public string? VoucherDescription { get; set; }
        public bool? IsQRStatic { get; set; }
    }
}
