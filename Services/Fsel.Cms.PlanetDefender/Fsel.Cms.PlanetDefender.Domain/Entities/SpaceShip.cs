// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class SpaceShip : Entity
    {
        public bool IsDefault { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
        public ICollection<StudentTagName> StudentTagNames { get; set; } = new List<StudentTagName>();
        public ICollection<SpaceShip> SpaceShips { get; set; } = new List<SpaceShip>();
    }
}
