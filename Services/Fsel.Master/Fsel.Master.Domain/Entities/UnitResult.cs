// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;
    using Fsel.Master.Domain.Models.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("UnitResult_Report", Schema = "dbo")]
    public class UnitResult
    {
        public Guid UnitResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? UnitId { get; set; }
        public EnumResultStatus? Status { get; set; }
        public decimal? Percent { get; set; }
        public int? TotalLessons { get; set; }
        public int? CompletedLessons { get; set; }
        public int? TotalTests { get; set; }
        public int? CompletedTests { get; set; }
        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
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
