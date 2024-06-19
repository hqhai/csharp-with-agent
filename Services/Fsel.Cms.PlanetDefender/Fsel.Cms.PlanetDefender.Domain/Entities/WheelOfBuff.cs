// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Entities;

    public class WheelOfBuff : Entity
    {
        public EnumWheelOfBuffType Type { get; set; }

        public bool IsActive { get; set; }

    }
}
