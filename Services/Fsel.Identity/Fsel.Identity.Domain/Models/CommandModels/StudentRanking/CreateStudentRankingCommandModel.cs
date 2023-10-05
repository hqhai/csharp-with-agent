// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentRanking
{
    public class CreateStudentRankingCommandModel
    {
        public Guid StudentId { get; set; }

        public int DailyStreak { get; set; }

        public int TotalScore { get; set; }

        public int PositionChange { get; set; }

        public int CurrentPosition { get; set; }
    }
}
