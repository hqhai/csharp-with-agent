// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels.V1i2
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchOrderModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int? MonthNumber { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public EnumOrderStatus Status { get; set; }
        public Guid UserId { get; set; }
        public Guid? PackageId { get; set; }
        public string? PackageName { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal Price { get; set; }
        public string? StudentCode { get; set; }
        public int? CountOrder { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentFullName { get; set; }
        public string? StudentPhoneNumber { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
