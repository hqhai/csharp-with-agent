// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumQuestBoardType
    {
        [Description("Nhiệm vụ chính")]
        MainQuests,

        [Description("Nhiệm vụ hàng ngày")]
        DailyQuests,

        [Description("Nhiệm vụ phụ")]
        SideQuests,

        [Description("Nhiệm vụ cao cấp")]
        PremiumQuests,

        [Description("Nhiệm vụ sự kiện")]
        EventQuests,

        [Description("Thợ săn kho báu")]
        TreasureHunters,

        [Description("Thử Thách Vui Nhộn")]
        FunChallenges
    }
}
