// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class LogActionDaysModel
    {
        public Guid Id { get; set; }
        public int NumberOfDaysStreak { get; set; }
        public bool IsDaysStreakIncrease { get; set; }
    }
}
