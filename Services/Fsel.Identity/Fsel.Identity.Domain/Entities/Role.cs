// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class Role : IdentityRole
    {
        [StringLength(500)]
        public string? Description { get; set; }

        public DateTimeOffset? ValidTo { get; set; }

    }
}
