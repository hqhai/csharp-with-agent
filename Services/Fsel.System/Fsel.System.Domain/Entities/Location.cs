// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel;

    public class Location : Entity
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }
        public EnumLocationType Type { get; set; }
        public Guid? ParentId { get; set; }
        public Location? Parent { get; set; }
        public int UrBoxId { get; set; }
        public string? IdPath { get; set; }
        public string? LocationName { get; set; }
        public string? LongPath { get; set; }
        public string? ShortPath { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        public ICollection<Location> Children { get; set; } = new List<Location>();
    }
}
