// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordCommandModel
    {
        public string? OldPassword { get; set; }

        [Required]
        public string? Password { get; set; }

        public Guid? UserId { get; set; }
    }
}
