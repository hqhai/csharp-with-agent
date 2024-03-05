// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    using Microsoft.AspNetCore.Http;

    public static class ValueSettings
    {
        public static class QuestBoardPoint
        {
            public const float Achieved_Point = 1; // QuestBoard chỉ thực hiện 1 lần thì chỉ tăng 1 Achieved_Point mỗi lần

            public const int Random_Daily_QuestBoard = 3; // Lấy ngẫu nhiên 3 nhiệm vụ trong daily questboard
        }

        public const int DelayOneMinute = 1;
        public const int DelayWorkerSecond = 60;
        public const int AmountTrialDays = 14;
        public static class CreateAction
        {
            public const string TwoPeopleLike = "{0},{1}";
            public const string ThreePeopleOrMoreLike = "{0} và {1} người khác";
            public const int NoOneAction = 0;
            public const int OnePeopleAction = 1;
            public const int TwoPeopleAction = 2;
        }
    }
}
