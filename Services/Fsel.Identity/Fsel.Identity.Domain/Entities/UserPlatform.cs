// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UserPlatform : Entity
    {
        public User? User { get; set; }
        public Guid UserId { get; set; }
        public Platform? Platform { get; set; }
        public Guid? PlatformId { get; set; }
        public EnumUserPlatformStatus Status { get; set; }
    }
}
