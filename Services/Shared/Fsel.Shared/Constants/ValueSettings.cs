// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class ValueSettings
    {
        public static class QuestBoardPoint
        {
            public const float Achieved_Point = 1; // QuestBoard chỉ thực hiện 1 lần thì chỉ tăng 1 Achieved_Point mỗi lần

            public const int Random_Daily_QuestBoard = 3; // Lấy ngẫu nhiên 3 nhiệm vụ trong daily questboard
        }

        public const int ValueDefault = 0;

        public const int DelayOneMinute = 1;
        public const int DelayThreeMinute = 3;
        public const int DelayTenMinutes = 10;
        public const int DelayTwoHours = 2;
        public const int DelayWorkerSecond = 60;
        public const int AmountTrialDays = 14;
        public const int AmountStudentSending = 1000;
        public const int MaxSendingRateApp = 3;
        public const int MinCompletePercent = 75;

        public const int Retrycount = 3;

        public const int BatchSize1000 = 1000; // Số lượng bản ghi mỗi lần truy vấn
        public const int BatchSize = 500; // Số lượng bản ghi mỗi lần truy vấn
        public const int BatchSize200 = 200; // Số lượng bản ghi mỗi lần truy vấn
        public const string FSEL_PUBLIC_FILES_URL = "https://s3-sgn10.fptcloud.com/fsel-public/Files/";

        public static class AgeMilestone
        {
            public const int ChildrenAge = 13;
            public const int StudentAge = 14;
            public const int TeenagersAge = 16;
        }

        public static class SectionGroupIELST
        {
            public const int ExecutionTimeReading = 3600;
            public const int AdditionalTimeListening = 120;

            public const int MaxSectionSkillWriting = 2;
            public const int MaxSectionSkillListening = 4;
            public const int MaxSectionSkillReading = 3;

            public const int MaxScoreSkillListening = 10;
            public const int MinScoreSkillReading = 13;
            public const int MaxScoreSkillReading = 14;
        }

        public static class CreateAction
        {
            public const string TwoPeopleLike = "{0},{1}";
            public const string ThreePeopleOrMoreLike = "{0} và {1} người khác";
            public const int NoOneAction = 0;
            public const int OnePeopleAction = 1;
            public const int TwoPeopleAction = 2;
        }

        public static class AcademicStudentResultRatio
        {
            public const double VideoRatio = 9;
            public const double UnitTestsRatio = 24;
            public const double SkillsTestsRatio = 18;
            public const double HomeWorkRatio = 14;
            public const double ClassForumRatio = 20;
            public const double FinalTestRatio = 15;
        }

        public static class IeltsStudentResultRatio
        {
            public const double VideoRatio = 20;
            public const double HomeWorkRatio = 48;
            public const double ClassForumRatio = 32;
        }

        public static class PromptNameTemplate
        {
            public const string ProgramName = "Program Name";
            public const string CourseName = "Course Name";
            public const string CEFRLevel = "Course CEFR Level";
            public const string UnitTopic = "Unit Topic";
            public const string UnitNumber = "Unit Number";
            public const string GrammarTopicList = "Grammar Topic List";
            public const string VocabularyLists = "Vocabulary Lists";
        }

        public static class ChatBotSetup
        {
            public const int RatioRound = 2; // làm tròn đến số thật phân
            public const int NumberDeletedElement = 2; // Số phần tử bị xóa đi trong mảng

            public const int Temperature = 0;
            public const string Model = "gpt-4o";
            public const string O4MINI = "o4-mini";
            public const int PresencePenalty = 0;
            public const int TopP = 0;
        }

        public static class StudentDailyStreak
        {
            public const int CheckInGoalTime = 900;
        }

        public static class CustomerSupport
        {
            public const string TitleMail = "Xử lý sự cố ChatGPT học sinh {0}";
            public const string Content = "Sự cố ChatGPT học sinh {0}\r\n\r\nVị trí gặp sự cố: {1}";
        }

        public static class TimeCodeStreak
        {
            public const int StreakFiveTimeCode = 5;
            public const int StreakTenTimeCode = 10;
            public const int StreakFifTeenTimeCode = 15;
            public const int StreakTwentyTimeCode = 20;
            public const int StreakTwentyFiveTimeCode = 25;
            public const int StreakThirtyTimeCode = 30;
            public const int StreakThirtyFiveTimeCode = 35;
            public const int StreakFourtyTimeCode = 40;
            public const int StreakFourtyFiveTimeCode = 45;
            public const int StreakFiftyTimeCode = 50;
        }

        public static class ExtendMonth
        {
            public const int TwentyFourMonth = 24;
            public const int TwelveMonth = 12;
            public const int SixMonth = 6;
            public const int ThreeMonth = 3;
        }

        public static class FselRatingValue
        {
            public const int DelayDateSendingRate = 5;
            public const int MoreThanOneDevice = 1;
        }

        public static class CourseProgressValue
        {
            public const int ProgressAcademic = 217;
            public const int ProgressIELTS = 106;
            public const int CountUnitAca = 12;
            public const int CountUnitIELTS = 8;
            public const int CountFullMockTest = 2;
            public const int CountFinalTest = 1;
            public const int MockTestPosition = 5;
            public const int SkillFullMocKTest = 4;
            public const int CountLessonAca = 6;
            public const int CountLessonIELTS = 4;
            public const int CountLessonRFI = 5;
            public const int CountUnitRFIA1 = 10;
            public const int CountUnitRFIA2 = 12;
        }

        public static class ValueOrderIndex
        {
            public const int OrderIndexProcess = 0;
            public const int OrderIndexNew = 1;
            public const int OrderIndexDone = 2;
            public const int OrderIndexOther = 3;
        }

        public static class ValueStatusUser
        {
            public const string CompletedPlacementTest = "Hoàn Thành PT";
            public const string NotCompletedPlacementTest = "Chưa Hoàn Thành PT";
            public const string InProgress = "Đang Học";
            public const string NotStarted = "Chưa Học";
        }

        public static class ValueCourseLevel
        {
            public const string PreA1 = "Pre-A1";
        }

        public static class OverallPercentCourse
        {
            #region Aca

            public const int OverallAcaPercentVideo = 9;
            public const int OverallAcaPercentUnitTest = 24;
            public const int OverallAcaPercentSkillTest = 18;
            public const int OverallAcaPercentHomeWork = 14;
            public const int OverallAcaPercentClassForum = 20;
            public const int OverallAcaPercentFinalTest = 15;

            #endregion Aca

            #region RFI

            public const int OverallRFIPercentVideo = 9;
            public const int OverallRFIPercentUnitTest = 42;
            public const int OverallRFIPercentHomeWork = 14;
            public const int OverallRFIPercentClassForum = 20;
            public const int OverallRFIPercentFinalTest = 15;

            #endregion RFI

            #region IELTS

            public const int OverallIELTSPercentVideo = 20;
            public const double SkillIELTSPercentVideo = 3.25;
            public const double SkillSWIELTSPercentVideo = 3.5;
            public const int OverallIELTSPercentHomeWork = 48;
            public const int OverallIELTSPercentClassForum = 32;

            #endregion IELTS
        }

        public static class OverallPercentUnit
        {
            #region Aca

            public const int OverallAcaPercentVideo = 18;
            public const int OverallAcaPercentUnitTest = 30;
            public const int OverallAcaPercentSkillTest = 10;
            public const int OverallAcaPercentHomeWork = 22;
            public const int OverallAcaPercentClassForum = 20;

            #endregion Aca

            #region RFI

            public const int OverallRFIPercentVideo = 18;
            public const int OverallRFIPercentUnitTest = 40;
            public const int OverallRFIPercentHomeWork = 22;
            public const int OverallRFIPercentClassForum = 20;

            #endregion RFI

            #region IELTS

            public const double SkillIELTSPercentVideo = 3.25;
            public const double SkillSWIELTSPercentVideo = 3.5;
            public const int OverallIELTSPercentHomeWork = 48;
            public const int OverallIELTSPercentClassForum = 32;

            #endregion IELTS
        }

        public static class AnswerLength
        {
            public const int ShortAnswerMaxLength = 255;    // Giới hạn câu trả lời ngắn
            public const int LongAnswerMaxLength = 4000;   // Giới hạn câu trả lời dài
            public const int MaxLengthDisplayOrder0 = 1800;
            public const int MaxLengthDisplayOrder1 = 3000;
            public const int Section0 = 0;
            public const int Section1 = 1;
        }

        public static class LanguageAIModule
        {
            public const string English = "en-US";
            public const string Vietnamese = "vi-VN";
        }
    }
}
