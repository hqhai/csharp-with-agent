// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class SignUpCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public EnumRoleRegister Role { get; set; }

        public string? ReferralCode { get; set; }

        public EnumPlatformCode? PlatformCode { get; set; }
    }
}
