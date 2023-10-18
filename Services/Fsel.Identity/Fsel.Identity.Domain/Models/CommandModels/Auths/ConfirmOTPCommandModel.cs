// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;

    public class ConfirmOTPCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [EmailValid]
        public string? Email { get; set; }

        [Required]
        public string? OTP { get; set; }
    }
}
