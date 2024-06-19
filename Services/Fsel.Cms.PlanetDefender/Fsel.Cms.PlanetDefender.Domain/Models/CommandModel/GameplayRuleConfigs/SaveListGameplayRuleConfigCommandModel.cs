// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayRuleConfigs
{
    using System;

    public class SaveListGameplayRuleConfigCommandModel
    {
        public IList<SaveGameplayRuleConfigCommandModel>? GameplayRuleConfigs { get; set; }
    }

    public class SaveGameplayRuleConfigCommandModel
    {
        public Guid? Id { get; set; }
        public int StartRoundNumber { get; set; }
        public int EndRoundNumber { get; set; }
        public int CurrentUnit { get; set; }
        public int CurrentUnitOutside { get; set; }
        public int PreviousUnit { get; set; }
        public int PreviousUnitOutside { get; set; }
        public int NumberQuestionPerGame { get; set; }
    }
}
