// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumQuestBoardCategory
    {
        [Description("Kết thúc một bài học")]
        FinishOneLesson,

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

        [Description("Post một bài class forum")]
        FinishOneClassForumPost,

        [Description("Bình luận vào một bài class forum của học sinh khác")]
        CommentOnOtherPost,

        [Description("Ba mươi phút / một lần")]
        ThirtyMinutesFocusMode,

        [Description("Sáu mươi phút / một lần")]
        SixtyMinutesFocusMode,

        [Description("Chín mươi phút / một lần")]
        NinetyMinutesFocusMode,

        [Description("Trăm hai mươi phút / một lần")]
        OneHundredTwentytyMinutesFocusMode,

        [Description("Trăm tám mươi phút / một Lần ")]
        OneHundredEightyMinutesFocusMode,
    }
}
