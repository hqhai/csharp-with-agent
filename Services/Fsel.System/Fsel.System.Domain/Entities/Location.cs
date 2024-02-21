// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Location : Entity
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }
        public EnumLocationType Type { get; set; }
        public Guid? ParentId { get; set; }
        public Location? Parent { get; set; }
        public int UrBoxId { get; set; }
        public ICollection<Location> Children { get; set; } = new List<Location>();
    }
}
