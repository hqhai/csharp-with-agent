// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordCommandModel
    {
        [Required]
        public string? OldPassword { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
