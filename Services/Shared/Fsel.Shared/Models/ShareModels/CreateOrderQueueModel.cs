// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class CreateOrderQueueModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? CodeCourse { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
    }
}
