// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class StudentRankingEventModel
    {
        public double? RankingScore { get; set; }

        public double? OverallScore { get; set; }

        public double? Process { get; set; }
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseResultId { get; set; }
    }
}
