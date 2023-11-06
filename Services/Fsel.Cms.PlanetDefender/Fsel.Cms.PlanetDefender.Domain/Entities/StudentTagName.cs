// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;

    public class StudentTagName : Entity
    {
        public int Level { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? TagName { get; set; }

        public Guid? MaxLevelSpaceShipId { get; set; }

        public SpaceShip? SpaceShip { get; set; }
    }
}
