// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class LeaderBoardQueueModel
    {
        public IList<StudentRankingRealTime>? StudentRankings { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class StudentRankingRealTime
    {
        public Guid StudentId { get; set; }

        public int DailyStreak { get; set; }

        public double TotalScore { get; set; }

        public int PositionChange { get; set; }

        public int CurrentPosition { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

    }
}
