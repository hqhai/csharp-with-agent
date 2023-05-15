// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordCommandModel
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
