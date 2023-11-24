// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameHistorys
{
    using System;

    public class SaveGameHistoryCommandModel
    {
        public Guid? Id { get; set; }
        public int RoundNumber { get; set; }
        public long Score { get; set; }
        public int NumberOfToken { get; set; }
        public int ImpactNumber { get; set; }
        public int DestroyNumber { get; set; }
        public int ComboNumber { get; set; }
        public int ZPlanetNumber { get; set; }
    }
}
