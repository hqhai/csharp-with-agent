// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class OrderSearchModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public EnumCourseLevel? CourseName { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public string? PackageName { get; set; }
        public EnumOrderStatus Status { get; set; }
        public Guid UserId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid PackageId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsTrial { get; set; }
        public int MonthNumber { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? StatusCourseResult { get; set; }
    }
}
