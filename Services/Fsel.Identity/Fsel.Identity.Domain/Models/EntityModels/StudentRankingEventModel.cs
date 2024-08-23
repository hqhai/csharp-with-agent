// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class StudentRankingEventModel : BaseModel
    {
        public string? Grade { get; set; }
        public double? RankingScore { get; set; }
        public double? OverallScore { get; set; }
        public double? Proccess { get; set; }
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseResultId { get; set; }
        public Guid CompetitionEventId { get; set; }
        public CompetitionEventsModel? CompetitionEvent { get; set; }
    }
}
