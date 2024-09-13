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

        public static class AgeMilestone
        {
            public const int ChildrenAge = 13;
            public const int StudentAge = 14;
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
            public const string Model = "gpt-4-turbo";
            public const int PresencePenalty = 0;
            public const int TopP = 0;
        }

        public static class StudentDailyStreak
        {
            public const int CheckInGoalTime = 54000;
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
    }
}