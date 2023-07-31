// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateOrderCommandModel
    {
        public string? CodeClass { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }

        public IList<DayOfWeek>? LiveDays { get; set; }
    }
}
