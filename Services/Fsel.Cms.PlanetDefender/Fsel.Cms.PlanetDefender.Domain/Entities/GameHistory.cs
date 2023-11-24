// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class GameHistory : Entity
    {
        public int RoundNumber { get; set; }
        public long Score { get; set; }
        public Guid StudentGameInfoId { get; set; }
        public Guid SpaceShipId { get; set; }
        public int NumberOfToken { get; set; }
        public int ImpactNumber { get; set; }
        public int DestroyNumber { get; set; }
        public int ComboNumber { get; set; }
        public int ZPlanetNumber { get; set; }
        public SpaceShip? SpaceShip { get; set; }
        public StudentGameInfo? StudentGameInfo { get; set; }
        public ICollection<GameAnswer> GameAnswers { get; set; } = new List<GameAnswer>();
    }
}
