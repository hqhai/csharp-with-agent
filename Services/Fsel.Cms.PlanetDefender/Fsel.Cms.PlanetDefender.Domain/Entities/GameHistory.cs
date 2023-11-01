// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class GameHistory : Entity
    {
        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int RoundNumber { get; set; }

        public long Score { get; set; }

        public Guid StudentId { get; set; }

        public Guid SpaceShipId { get; set; }

        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int CoinNumber { get; set; }

        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int ImpactNumber { get; set; }

        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int DestroyNumber { get; set; }

        public SpaceShip? SpaceShip { get; set; }
    }
}
