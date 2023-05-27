// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Ordering.Domain.Enums;

    public class CreateOrderCommandModel
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public EnumOrderStatus Status { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public double Price { get; set; }
        public double DiscountPercent { get; set; }
        public double DiscountPrice { get; set; }
        public double TotalPrice { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
    }
}
