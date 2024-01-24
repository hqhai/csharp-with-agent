// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    public class OrderXMLModel
    {
        public string? UserName { get; set; }
        public string? ShopId { get; set; }
        public string? Session { get; set; }
        public string? ShopDomain { get; set; }
        public string? ShopBackUrl { get; set; }
        public string? OrderNo { get; set; }
        public int OrderCashAmount { get; set; }
        public string? OrderShipDate { get; set; }
        public int OrderShipDays { get; set; }
        public string? OrderDescription { get; set; }
        public string? NotifyUrl { get; set; }
        public string? ValidityTime { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
