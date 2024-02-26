// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class UpdateOrderCommandModel
    {
        public Order? Order { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid PackageId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
    }
}
