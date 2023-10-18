// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using Fsel.Shared.Enums;

    public class OrderModel
    {
        public Guid Id { get; set; }
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
        public PackageModel? Package { get; set; }
        public Guid UserId { get; set; }
    }
}
