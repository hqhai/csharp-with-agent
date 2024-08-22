// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentRanking
{
    public class StudentRankingEventCommandModel
    {
        public double? RankingScore { get; set; }
        public double? Process { get; set; }
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseResultId { get; set; }
        public double? OverallScore { get; set; }

    }
}
