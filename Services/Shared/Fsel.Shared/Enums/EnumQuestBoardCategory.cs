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

        [Description("Xem phản 5 hồi giáo viên")]
        SeeFiveTeacherReview,

        [Description("Xem phản 10 hồi giáo viên")]
        SeeTenTeacherReview,

        [Description("Xem phản all hồi giáo viên")]
        SeeAllTeacherReview,

        [Description("Viết 1 bài discussion board")]
        PostOneDiscussionBoard,

        [Description("Viết 3 bài discussion board")]
        PostThreeDiscussionBoard,

        [Description("Viết 5 bài discussion board")]
        PostFiveDiscussionBoard,

        [Description("Điểm tham gia")]
        ParticipationScore,

        [Description("30 phút 1 lần")]
        ThirtyMinutesFocusMode,

        [Description("60 phút 1 lần")]
        SixtyMinutesFocusMode,

        [Description("90 phút 1 lần")]
        NinetyMinutesFocusMode,

        [Description("120 phút 1 lần")]
        OneHundredTwentytyMinutesFocusMode,

        [Description("180 phút 1 lần")]
        OneHundredEightyMinutesFocusMode,
    }
}
