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

        [Description("Focus Mode I: Bạn cần lựa chọn mốc tập trung 30 phút và hoàn thành mốc tập trung này một lần")]
        ThirtyMinutesFocusMode,

        [Description("Focus Mode II: Bạn cần lựa chọn mốc tập trung 60 phút và hoàn thành mốc tập trung này một lần")]
        SixtyMinutesFocusMode,

        [Description("Focus Mode III: Bạn cần lựa chọn mốc tập trung 90 phút và hoàn thành mốc tập trung này một lần")]
        NinetyMinutesFocusMode,

        [Description("Focus Mode IV: Bạn cần lựa chọn mốc tập trung 120 phút và hoàn thành mốc tập trung này một lần")]
        OneHundredTwentytyMinutesFocusMode,

        [Description("Focus Mode V: Bạn cần lựa chọn mốc tập trung 180 phút và hoàn thành mốc tập trung này một lần")]
        OneHundredEightyMinutesFocusMode,
    }
}
