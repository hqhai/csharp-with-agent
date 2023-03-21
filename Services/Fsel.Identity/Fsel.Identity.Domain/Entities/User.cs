// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Domain.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        public virtual Human? Human { get; set; }
    }
}
