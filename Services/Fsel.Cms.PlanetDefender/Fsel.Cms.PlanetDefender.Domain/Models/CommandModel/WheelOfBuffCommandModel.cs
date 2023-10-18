// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Enums;

    public class WheelOfBuffCommandModel
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }

        public EnumWheelOfBuffType Type { get; set; }

    }
}
