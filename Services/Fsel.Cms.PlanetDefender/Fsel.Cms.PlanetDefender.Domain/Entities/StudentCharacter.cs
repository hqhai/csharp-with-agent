// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class StudentCharacter : Entity
    {
        public bool IsActive { get; set; }
        public Guid StudentGameInfoId { get; set; }
        public Guid CharacterId { get; set; }
        public Character? Character { get; set; }
        public StudentGameInfo? StudentGameInfo { get; set; }
    }
}
