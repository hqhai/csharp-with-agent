// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SpaceShipModel : BaseModel
    {
        public bool IsDefault { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
