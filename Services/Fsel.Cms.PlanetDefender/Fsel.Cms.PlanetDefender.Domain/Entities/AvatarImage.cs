// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Core.Entities;

    public class AvatarImage : Entity
    {
        public bool IsActive { get; set; }

        public string? FilePath { get; set; }

        public int? Level { get; set; }
    }
}
