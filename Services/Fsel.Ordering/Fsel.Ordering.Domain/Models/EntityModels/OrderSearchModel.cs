// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class OrderSearchModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int? MonthNumber { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public EnumOrderStatus Status { get; set; }
        public Guid UserId { get; set; }
        public Guid? PackageId { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}
