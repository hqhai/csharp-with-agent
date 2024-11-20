// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserSchool : Entity
    {
        public Guid SchoolId { get; set; }
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
