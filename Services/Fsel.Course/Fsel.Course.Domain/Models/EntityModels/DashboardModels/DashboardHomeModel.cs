// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.DashboardModels
{
    public class DashboardHomeModel
    {
        public int CountCompleteLesson { get; set; }
        public int TotalLesson { get; set; }
        public long NumberOfDaysStreak { get; set; }
        public long TotalCoin { get; set; }
        public bool IsDaysStreakIncrease { get; set; }
        public int TotalNote { get; set; }
        public LessonResultModel? LessonResult { get; set; }
    }
}
