// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Shared.Enums;

    public class CreateOrderCommandModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? CodeCourse { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        public Guid UserId { get; set; }
    }
}
