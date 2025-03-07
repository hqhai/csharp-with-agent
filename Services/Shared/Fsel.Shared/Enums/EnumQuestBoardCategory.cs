// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumQuestBoardCategory
    {
        #region BeginnerQuests

        [Description("Hoàn thành bài kiểm tra đầu tiên của bạn")]
        CompleteTheFirstTest,

        [Description("Hoàn thành Video bài học đầu tiên")]
        CompleteTheFirstVideoLesson,

        [Description("Hoàn thành Diễn đàn lớp học đầu tiên")]
        CompleteTheFirstClassForum,

        [Description("Hoàn thành Bài tập về nhà đầu tiên")]
        CompleteHomeworkFirst,

        [Description("Hoàn thành Khảo sát thông tin")]
        CompletedSurvey,

        [Description("Hoàn thành Mục tiêu học tập ngày đầu tiên")]
        CompleteFocusModeFirst,

        [Description("Hoàn thành Unit đầu tiên")]
        CompleteTheFirstUnit,

        #endregion BeginnerQuests

        #region LearningQuests

        [Description("Tương tác tính năng \"Học\" trong 20 phút")]
        ExploreTheLearningGalaxy,

        [Description("Nhận xét hoặc Thích một bài viết trong Diễn đàn chung")]
        InterstellarInteractions,

        [Description("Hoàn thành mốc tập trung của hôm nay")]
        CompleteMissionDay,

        [Description("Hoàn thành 1 bài tập về nhà với điểm tổng kết trên 50%")]
        ConqueringAsteroids,

        [Description("Đăng tải bình luận trên bài viết thuộc Diễn đàn lớp học gần nhất")]
        ConnectingAllies,

        [Description("Xem lại nôi dung bạn đã ghi chú")]
        BackupNotes,

        [Description("Tạo một bài đăng trên Diễn đàn chung")]
        SharedRocketLaunch,

        [Description("Tạo một ghi chú mới")]
        GalaxyNotes,

        [Description("Hoàn thành một câu hỏi trong Video bài học")]
        DecodingTheNebula,

        [Description("Xem lại một Video bài học đã hoàn thành")]
        HistoryOfDiscovery,

        [Description("Thời gian truy cập tính năng \"Học\" đạt 120 phút")]
        LearningSpaceship,

        [Description("Hoàn thành 20 câu hỏi trong Video bài học")]
        JourneyOfKnowledge,

        [Description("Xem phản hồi của AI cho bài đăng Class Forum của bạn")]
        MessagesFromAI,

        [Description("Hoàn thành 7 lần Mục tiêu học tập ngày")]
        InfinityFocusMode,

        [Description("Hoàn thành 10 bài tập về nhà")]
        TheMysteryOfTheStars

        #endregion LearningQuests
    }
}
