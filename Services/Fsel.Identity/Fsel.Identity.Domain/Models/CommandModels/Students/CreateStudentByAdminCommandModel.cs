// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System;

    public class CreateStudentByAdminCommandModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public Guid? SchoolId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ReferralCode { get; set; }
    }
}
