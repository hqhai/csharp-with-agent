// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;
    using Fsel.Master.Domain.Models.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("PlacementTest_Report", Schema = "dbo")]
    public class PlacementTest
    {
        public Guid TestResultId { get; set; }
        public Guid TestGroupResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid TestId { get; set; }
        public EnumResultStatus? Status { get; set; }
        public decimal? ScorePercent { get; set; }
        public int? TotalQuestions { get; set; }
        public int? CorrectCount { get; set; }
        public int? AttemptNumber { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? SkillScoresJson { get; set; }

        [NotMapped]
        public IList<SkillScore>? SkillScores
        {
            get
            {
                return !string.IsNullOrEmpty(SkillScoresJson) ? JsonSerializer.Deserialize<IList<SkillScore>>(SkillScoresJson) : null;
            }
        }
    }
}
