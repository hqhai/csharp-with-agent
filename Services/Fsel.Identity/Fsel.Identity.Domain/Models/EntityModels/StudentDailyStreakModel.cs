// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class StudentDailyStreakModel
    {
        public int CountStudentDaily { get; set; }
        public int NumberOfShield { get; set; }
        public int NumberOfGift { get; set; }
        public IList<StudentConsecutiveDayModel>? DailyDayOfGifts { get; set; }
    }
}
