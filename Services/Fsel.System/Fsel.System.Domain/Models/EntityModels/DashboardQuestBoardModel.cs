// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using global::System.Collections.Generic;

    public class DashboardQuestBoardModel
    {
        public int CurrentValue { get; set; }
        public EnumQuestBoardType Type { get; set; }
        public IList<QuestBoardOverallModel> QuestBoardOveralls { get; set; } = new List<QuestBoardOverallModel>();
        public IList<QuestBoardModel> QuestBoardModels { get; set; } = new List<QuestBoardModel>();
    }
}
