// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class OrderIntegrationModel : BaseModel
    {
        public string? OrderCode { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Program { get; set; }

        public string? CourseLevel { get; set; }

        public int CoursePackage { get; set; }

        public EnumPaymentMethodStatus PaymentMethod { get; set; }

        public decimal DiscountPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public EnumOrderStatus? Status { get; set; }

        public string? StatusCourseResult { get; set; }

        public EnumPaymentRevenueType? RevenueType { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public double Price { get; set; }

        public string? ReferralCode { get; set; }

        public string? Voucher { get; set; }

        public bool IsTrial { get; set; }
    }

    public class VoucherIntegrationModel
    {
        public string? Code { get; set; }
        public string? CodePrefix { get; set; }
        public string? Name { get; set; }
        public EnumVoucherCategory Category { get; set; }
        public int Value { get; set; }
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SourceName { get; set; }
        public bool IsActive { get; set; }
        public int? NumberOfChanges { get; set; }
    }
}
