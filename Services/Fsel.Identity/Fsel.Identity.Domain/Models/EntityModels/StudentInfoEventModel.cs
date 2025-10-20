// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class StudentInfoEventModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string? School { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? ParentEmail { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool IsChangePassword { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsStudentVerifiedForEvent { get; set; }
        public bool IsParent { get; set; }
        public bool AllowParentInfoUpdate { get; set; }
        public CompanionInfoEventModel? CompanionInfo { get; set; }
    }
}
