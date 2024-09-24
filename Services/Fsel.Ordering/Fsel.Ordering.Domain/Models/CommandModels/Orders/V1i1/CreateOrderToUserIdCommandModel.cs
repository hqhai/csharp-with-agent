// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1
{
    using Fsel.Shared.Enums;

    public class CreateOrderToUserIdCommandModel
    {
        public Guid UserId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid? PackageId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid CourseId { get; set; }
        public bool IsTrialRegistration { get; set; }
        public EnumPaymentRevenueType RevenueType { get; set; }
    }
}
