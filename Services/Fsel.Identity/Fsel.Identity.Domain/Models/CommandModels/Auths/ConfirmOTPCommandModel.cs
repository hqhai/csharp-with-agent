// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using Fsel.Shared.Enums;
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
        public EnumGender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        [Required]
        public string? OTP { get; set; }
    }
}
