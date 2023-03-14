// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;

    public class UserRole : IdentityUserRole<string>
    {
        public int Id { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
