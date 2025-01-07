// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System;

    public class CreateUserByAdminCommandModel
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
