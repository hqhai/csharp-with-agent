// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class StudentTagName : Entity
    {
        public int Level { get; set; }

        public string? TagName { get; set; }

        public Guid? MaxLevelSpaceShipId { get; set; }

        public SpaceShip? SpaceShip { get; set; }
    }
}
