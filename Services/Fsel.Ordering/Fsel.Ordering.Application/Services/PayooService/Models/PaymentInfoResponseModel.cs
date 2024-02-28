// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    public class PaymentInfoResponseModel
    {
        public string? PaymentMethod { get; set; }
        public string? PurchaseDate { get; set; }
        public int? ShopId { get; set; }
        public int? MasterShopId { get; set; }
        public string? OrderNo { get; set; }
        public double? OrderCash { get; set; }
        public int? CardIssuanceType { get; set; }
        public int? PaymentStatus { get; set; }
        public string? CustomerIdentifier { get; set; }
        public string? MDD1 { get; set; }
        public string? MDD2 { get; set; }
        public string? PYTransId { get; set; }
        public string? PaymentMethodName { get; set; }
    }
}
