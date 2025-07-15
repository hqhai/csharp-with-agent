// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class LoginCommandModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [MaxLength(254, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [MinLength(8, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public string? Password { get; set; }

        public EnumPlatformCode PlatformCode { get; set; } = EnumPlatformCode.LMS;
    }
}
