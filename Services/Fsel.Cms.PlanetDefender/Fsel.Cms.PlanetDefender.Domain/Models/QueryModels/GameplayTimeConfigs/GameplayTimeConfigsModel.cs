// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayTimeConfigs
{
    using Fsel.Shared.Enums;

    public class GameplayTimeConfigsModel
    {
        public int RoundNumber { get; set; }
        public IList<GameplayTimeConfigModel>? GameplayTimeConfigs { get; set; }
    }

    public class GameplayTimeConfigModel
    {
        public Guid Id { get; set; }
        public int RoundNumber { get; set; }
        public EnumGameVocabPDType GameVocabPDType { get; set; }
        public double Time { get; set; }
        public double Percent { get; set; }
    }
}
