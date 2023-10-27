// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentRanking : Entity
    {
        public Guid StudentId { get; set; }

        public int DailyStreak { get; set; }

        public double TotalScore { get; set; }

        public int PositionChange { get; set; }

        public int CurrentPosition { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
