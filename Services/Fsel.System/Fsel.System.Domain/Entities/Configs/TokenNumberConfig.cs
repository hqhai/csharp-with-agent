// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    using Fsel.Shared.Enums;

    public class TokenNumber
    {
        public int? Number { get; set; }
    }

    public class TokenForcusTime
    {
        public IList<FocusTimeNumber>? FocusTimes { get; set; }
    }

    public class FocusTimeNumber
    {
        public Guid FocusTimeId { get; set; }
        public int? Number { get; set; }
    }

    public class TokenDailyCheckin
    {
        public IList<DailyCheckinNumber>? DailyCheckins { get; set; }
    }


    public class DailyCheckinNumber
    {
        public int Level { get; set; }
        public int? Number { get; set; }
    }

    public class TokenQuestBoard
    {
        public IList<QuestBoardNumber>? QuestBoards { get; set; }
    }

    public class QuestBoardNumber
    {
        public int? Number { get; set; }
        public EnumQuestBoardType QuestBoardType { get; set; }
    }

    public class TokenAchievement
    {
        public IList<AchievementNumber>? Achievements { get; set; }
    }

    public class AchievementNumber
    {
        public int? Number { get; set; }
        public EnumAchievementType AchievementType { get; set; }
    }

    public class TimeCodeSuperFire
    {
        public int? TimeCode { get; set; }

        public int? Test { get; set; }
        public int? MockTest { get; set; }
    }

    public class TestComple
    {
        public int? Test { get; set; }
        public int? MockTest { get; set; }
    }

    public class AchievementToken
    {
        public int? Id { get; set; }
        public int? Number { get; set; }
    }

    public class BuyCourseStandad
    {
        public int? NumberStandard { get; set; }
        public int? NumberPremium { get; set; }
    }
    public class TestDone
    {
        public int? Test { get; set; }
        public int? MockTest { get; set; }
    }

    public class QuestionReward
    {
        public int? QuestionUngraded { get; set; }
        public int? SubQuestion { get; set; }
    }
}
