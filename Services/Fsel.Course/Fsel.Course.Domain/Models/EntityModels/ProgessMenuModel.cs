// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ProgessMenuModel
    {
        public long NumberOfUnitDone { get; set; }
        public long NumberOfDaysStreak { get; set; }
        public bool IsDaysStreakIncrease { get; set; }
        public long NumberOfQuestionDone { get; set; }
        public long NumberOfPostsCreated { get; set; }
        public long NumberOfPracticesDone { get; set; }
    }
}
