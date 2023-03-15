// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class Role : IdentityRole
    {
        [MaxLength(250)]
        public string? Discription { get; set; }
    }
}
