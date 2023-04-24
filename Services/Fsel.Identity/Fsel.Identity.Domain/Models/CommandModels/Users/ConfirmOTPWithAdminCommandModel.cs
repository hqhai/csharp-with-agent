// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ConfirmOTPWithAdminCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public object? CourseTypes { get; set; }
        public object? CourseLevels { get; set; }

        [Required]
        public EnumGender? Gender { get; set; }

        [Required]
        public EnumRoleRegisterWithAdmin Role { get; set; }

        public DateTime? Birthday { get; set; }
        public string? OTP { get; set; }
    }
}
