// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class StudentSpaceShipModel : BaseModel
    {
        public bool IsActive { get; set; }

        public int Level { get; set; }

        public Guid StudentId { get; set; }

        public Guid SpaceShipId { get; set; }
    }
}
