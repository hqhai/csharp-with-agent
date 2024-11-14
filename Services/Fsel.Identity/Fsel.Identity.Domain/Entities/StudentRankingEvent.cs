// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentRankingEvent : Entity
    {
        public string? Grade { get; set; }
        public double? RankingScore { get; set; }

        public double? OverallScore { get; set; }

        public double? Process { get; set; }
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseResultId { get; set; }

        public EnumCourseType CourseType { get; set; }
    }
}
