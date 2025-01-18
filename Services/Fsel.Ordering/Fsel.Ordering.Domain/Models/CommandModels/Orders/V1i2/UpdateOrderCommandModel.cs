// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    using Fsel.Ordering.Domain.Entities;

    public class UpdateOrderCommandModel
    {
        public Order? Order { get; set; }
        public CreateOrderCommandModel? Request { get; set; }
        public string? StudentCode { get; set; }
        public int DiscountPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal Price { get; set; }
        public Guid? VoucherId { get; set; }
    }
}
