// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserRole : UserRoleEntity
    {
        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
