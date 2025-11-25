// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UserPublicModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? CourseName { get; set; }
        public bool IsDefaultPackage { get; set; }
    }
}
