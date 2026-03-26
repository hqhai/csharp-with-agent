// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Fsel.Master.Identity.Domain.Models.CommandModels
{
    public class RefreshTokenCommandModel
    {
        [Required]
        public string? AccessToken { get; set; }

        [Required]
        public string? RefreshToken { get; set; }
    }
}
