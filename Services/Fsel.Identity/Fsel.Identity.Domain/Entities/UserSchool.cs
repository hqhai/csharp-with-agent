// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserSchool : Entity
    {
        public Guid SchoolId { get; set; }
        public Guid UserId { get; set; }
        public string? SchoolName { get; set; }
        public string? LocalId { get; set; }
        public string? City { get; set; }
        public string? EventCode { get; set; }
        public virtual User? User { get; set; }
    }
}
