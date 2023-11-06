// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class GameHistoryModel : BaseModel
    {
        public int RoundNumber { get; set; }

        public long Score { get; set; }

        public Guid SpaceShipId { get; set; }

        public string? SpaceShipCode { get; set; }
        public int NumberOfToken { get; set; }
        public int ImpactNumber { get; set; }
        public int DestroyNumber { get; set; }
    }
}
