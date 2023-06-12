// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;

    public class OrderSearchModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel? CourseName { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public string? PackageName { get; set; }
        public EnumOrderStatus Status { get; set; }
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
    }
}
