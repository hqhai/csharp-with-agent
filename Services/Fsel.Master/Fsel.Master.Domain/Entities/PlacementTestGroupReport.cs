// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Fsel.Master.Domain.Models.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("PlacementTestGroup_Report", Schema = "dbo")]
    public class PlacementTestGroupReport
    {
        public Guid? TestGroupResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? ProgramId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public decimal? ScorePercent { get; set; }

        public int? TotalQuestions { get; set; }

        public int? CorrectCount { get; set; }

        public DateTime? CompletionDate { get; set; }

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

    public class SkillScore
    {
        [JsonPropertyName("skillId")]
        public Guid? SkillId { get; set; }

        [JsonPropertyName("skillName")]
        public string? SkillName { get; set; }

        [JsonPropertyName("skillFilePath")]
        public string? SkillFilePath { get; set; }

        [JsonPropertyName("scores")]
        public double? Scores { get; set; }

        [JsonPropertyName("percent")]
        public double? Percent { get; set; }

        [JsonPropertyName("skill")]
        public string? Skill { get; set; }

        [JsonPropertyName("totalQuestion")]
        public int? TotalQuestion { get; set; }

        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }

        [JsonPropertyName("correctCount")]
        public int? CorrectCount { get; set; }

        [JsonPropertyName("correctQuestion")]
        public int? CorrectQuestion { get; set; }

        [JsonPropertyName("countQuestion")]
        public int? CountQuestion { get; set; }
    }
}
