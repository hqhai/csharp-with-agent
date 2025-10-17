// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserRole : UserRoleEntity
    {
        public bool IsActive { get; set; } = true;
    }
}
