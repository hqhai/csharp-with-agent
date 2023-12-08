// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    using Fsel.Shared.Enums;

    public class TokenNumber
    {
        public int? Number { get; set; }
    }

    public class TokenFocusTimeNumber
    {
        public Guid FocusTimeId { get; set; }
        public int? Number { get; set; }
    }

    public class TokenDailyCheckinNumber
    {
        public int Level { get; set; }
        public int? Number { get; set; }
    }

    public class TokenQuestBoard
    {
        public int? Number { get; set; }
        public EnumQuestBoardType QuestBoardType { get; set; }
    }

    public class TokenAchievement
    {
        public int? Number { get; set; }
        public EnumAchievementType AchievementType { get; set; }
    }
}
