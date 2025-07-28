// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumTokenMission
    {
        TimeCodeFirstSubmit,
        TimeCodeSecondSubmit,
        ClassForumWriting,
        ClassForumSpeakingAudio,
        ClassForumSpeakingVideo,
        HomeworkFirstSubmit,
        HomeworkSecondSubmit,

        FocusMode,
        FocusModeFifteenMinutes,
        FocusModeThirtyMinutes,
        FocusModeFortyFiveMinutes,
        FocusModeSixtyMinutes,
        FocusModeNinetyMinutes,

        DailyCheckin,
        DailyCheckinLevelOne,
        DailyCheckinLevelTwo,
        DailyCheckinLevelThree,

        SkillTest,
        UnitTest,
        FinalTest,

        SkillMockTestReading,
        SkillMockTestListening,
        SkillMockTestSpeakingFC,
        SkillMockTestSpeakingLR,
        SkillMockTestSpeakingGRA,
        SkillMockTestSpeakingPron,
        SkillMockTestWritingTask1Work150,
        SkillMockTestWritingTask1TA,
        SkillMockTestWritingTask1CC,
        SkillMockTestWritingTask1LR,
        SkillMockTestWritingTask1GRA,
        SkillMockTestWritingTask2Work250,
        SkillMockTestWritingTask2TA,
        SkillMockTestWritingTask2CC,
        SkillMockTestWritingTask2LR,
        SkillMockTestWritingTask2GRA,

        FullMockTestReading,
        FullMockTestListening,
        FullMockTestSpeakingFC,
        FullMockTestSpeakingLR,
        FullMockTestSpeakingGRA,
        FullMockTestSpeakingPron,
        FullMockTestWritingTask1Work150,
        FullMockTestWritingTask1TA,
        FullMockTestWritingTask1CC,
        FullMockTestWritingTask1LR,
        FullMockTestWritingTask1GRA,
        FullMockTestWritingTask2Work250,
        FullMockTestWritingTask2TA,
        FullMockTestWritingTask2CC,
        FullMockTestWritingTask2LR,
        FullMockTestWritingTask2GRA,
        QuestionCompleted,
        PhaseI,
        PhaseII,
        PhaseIII,
        PhaseIV,
        PhaseV,
        PhaseVI,
        SurveyEvent,
        RecallCoinsSurveyEvent,
        ReclaimGiftCoins,
        SurveyReward,
        FselEventReward,

        #region QuestBoard

        [Description("Hoàn thành bài kiểm tra đầu tiên của bạn")]
        CompleteTheFirstTest,

        [Description("Hoàn thành Video bài học đầu tiên")]
        CompleteTheFirstVideoLesson,

        [Description("Hoàn thành Diễn đàn lớp học đầu tiên")]
        CompleteTheFirstClassForum,

        [Description("Hoàn thành Bài tập về nhà đầu tiên")]
        CompleteHomeworkFirst,

        [Description("Hoàn thành Mục tiêu học tập ngày đầu tiên")]
        CompleteFocusModeFirst,

        [Description("Hoàn thành Unit đầu tiên")]
        CompleteTheFirstUnit,

        [Description("Hoàn thành hai nhiệm vụ tân thủ")]
        CompleteTwoBeginnerMissions,

        [Description("Hoàn thành bốn nhiệm vụ tân thủ")]
        CompleteFourBeginnerMissions,

        [Description("Hoàn thành sáu nhiệm vụ tân thủ")]
        CompleteSixBeginnerMissions,

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
        TheMysteryOfTheStars,

        [Description("Hoàn thành nhiệm vụ tuần")]
        CompleteWeeklyTasks,

        #endregion QuestBoard

        #region Friend mission

        FriendCompletePT,
        FriendCompleteUnit1,
        FriendCompletePayment,

        #endregion Friend mission

        #region
        FselStore,
        UrBox,
        #endregion

        #region Blind Box

        [Description("Mở rương mảnh ghép số 1")]
        PurchaseChestFirst,

        [Description("Mở rương mảnh ghép số 2")]
        PurchaseChestSecond,

        [Description("Mở rương mảnh ghép số 3")]
        PurchaseChestThird,

        [Description("Mở rương mảnh ghép số 4")]
        PurchaseChestFourth,

        [Description("Mở rương mảnh ghép số 5")]
        PurchaseChestFifth,

        [Description("Mở rương mảnh ghép số 6")]
        PurchaseChestSixth,

        [Description("Mở rương nhận Fsel coins")]
        OpenChestCoins,

        #endregion

        MarketPlacePremium
    }
}
