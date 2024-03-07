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

        public const int DelayOneMinute = 1;
        public const int DelayWorkerSecond = 60;

        public static class AcademicStudentResultRatio
        {
            public const double VideoRatio = 9;
            public const double UnitTestsRatio = 24;
            public const double SkillsTestsRatio = 18;
            public const double HomeWorkRatio = 14;
            public const double ClassForumRatio = 20;
            public const double FinalTestRatio = 15;
        }

    }
}
