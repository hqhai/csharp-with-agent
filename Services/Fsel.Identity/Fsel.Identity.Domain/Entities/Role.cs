// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;

    public class Role : IdentityRole<Guid>
    {
        [Required]
        [MaxLength(250)]
        public string? Discription { get; set; }
    }
}
