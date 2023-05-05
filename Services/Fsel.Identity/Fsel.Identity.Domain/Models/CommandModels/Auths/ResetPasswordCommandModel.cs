// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class ResetPasswordCommandModel
    {
        public string? Email { get; set; }

        [Required]
        public string? OldPassword { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
