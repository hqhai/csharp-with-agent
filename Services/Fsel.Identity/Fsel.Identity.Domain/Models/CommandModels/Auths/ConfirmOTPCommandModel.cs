// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using Fsel.Common.Enums;
    using Fsel.Identity.Domain.Enums;
    using System.ComponentModel.DataAnnotations;

    public class ConfirmOTPCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }

        [Required]
        public EnumGender? Gender { get; set; }

        [Required]
        public EnumRoleRegister Role { get; set; }

        public DateTime? Birthday { get; set; }
        public string? Code { get; set; }
        public string? OTP { get; set; }
    }
}
