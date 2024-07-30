// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models.QueryModels
{
    public class StudentDailyStreakCommandModel
    {
        public Guid? StudentId { get; set; }

        public bool IsUseShield { get; set; }

        public DateTime? DailyDate { get; set; }

        public Guid UserId { get; set; }
    }
}
