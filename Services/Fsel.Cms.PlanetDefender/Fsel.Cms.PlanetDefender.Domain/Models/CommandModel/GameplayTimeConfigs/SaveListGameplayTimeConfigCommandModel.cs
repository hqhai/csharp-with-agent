// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayTimeConfigs
{
    using Fsel.Shared.Enums;

    public class SaveListGameplayTimeConfigCommandModel
    {
        public IList<SaveGameplayTimeConfigCommandModel>? GameplayTimeConfigs { get; set; }
    }

    public class SaveGameplayTimeConfigCommandModel
    {
        public Guid? Id { get; set; }
        public int RoundNumber { get; set; }
        public EnumGameVocabPDType GameVocabPDType { get; set; }
        public double Time { get; set; }
        public double Percent { get; set; }
    }
}
