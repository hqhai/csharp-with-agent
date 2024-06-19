// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayRuleConfigs
{
    using Fsel.Core.Base.BaseModels;

    public class GameplayRuleConfigsModel : BaseModel
    {
        public int StartRoundNumber { get; set; }
        public int EndRoundNumber { get; set; }
        public int CurrentUnit { get; set; }
        public int CurrentUnitOutside { get; set; }
        public int PreviousUnit { get; set; }
        public int PreviousUnitOutside { get; set; }
        public int NumberQuestionPerGame { get; set; }
    }
}
