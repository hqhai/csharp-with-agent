// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class StudentTagNameModel : BaseModel
    {
        public int Level { get; set; }

        public string? TagName { get; set; }

        public Guid? MaxLevelSpaceShipId { get; set; }

        public SpaceShipModel? SpaceShip { get; set; }
    }
}
