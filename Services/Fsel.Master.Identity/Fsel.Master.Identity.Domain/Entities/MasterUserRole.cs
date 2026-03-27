// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Entities;

namespace Fsel.Master.Identity.Domain.Entities
{
    public class MasterUserRole : UserRoleEntity
    {
        public bool IsActive { get; set; } = true;
    }
}
