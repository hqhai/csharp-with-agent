// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class OrderModel : BaseModel
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public EnumOrderStatus Status { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public double Price { get; set; }
        public double DiscountPercent { get; set; }
        public double DiscountPrice { get; set; }
        public double TotalPrice { get; set; }
        public bool IsTrial { get; set; }
        public Guid CourseId { get; set; }
        public PackageModel? Package { get; set; }
        public Guid UserId { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}
