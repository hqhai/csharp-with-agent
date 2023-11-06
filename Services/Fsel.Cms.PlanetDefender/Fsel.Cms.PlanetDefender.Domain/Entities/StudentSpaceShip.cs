// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentSpaceShip : Entity
    {
        public bool IsActive { get; set; }

        public int Level { get; set; }

        public Guid StudentGameInfoId { get; set; }

        public Guid SpaceShipId { get; set; }

        public SpaceShip? SpaceShip { get; set; }

        public StudentGameInfo? StudentGameInfo { get; set;}
    }
}
