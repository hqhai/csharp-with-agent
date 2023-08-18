// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumQuestBoardCategory
    {
        [Description("Kết thúc một bài học")]
        FinishOnelesson,

        [Description("Hoàn thành một bài tập về nhà/dự án nhỏ")]
        FinishOneHomeworkMiniProject,

        [Description("Hoàn thành một bài kiểm tra Đơn vị")]
        FinishOneUnitTest,

        [Description("Hoàn thành một đơn vị")]
        FinishOneUnit,

        [Description("Kết thúc bài kiểm tra cuối kỳ đầu tiên")]
        FinishOneFinalTest,

        [Description("Hoàn thành Cấp độ đầu tiên (Đạt)")]
        FinishOneLevelPass,
    }
}
