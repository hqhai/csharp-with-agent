// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel;
    using global::System.ComponentModel.DataAnnotations;

    public class Location : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public int Level { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public EnumLocationType Type { get; set; }
        public Guid? ParentId { get; set; }
        public Location? Parent { get; set; }
        public int UrBoxId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? IdPath { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? LocationName { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? LongPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ShortPath { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        public ICollection<Location> Children { get; set; } = new List<Location>();
        public ICollection<School> Schools { get; set; } = new List<School>();
    }
}
