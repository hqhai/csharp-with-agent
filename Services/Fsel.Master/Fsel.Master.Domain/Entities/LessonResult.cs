// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;
    using Fsel.Master.Domain.Models.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("LessonResult_Report", Schema = "dbo")]
    public class LessonResult
    {
        public Guid LessonResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid LessonId { get; set; }
        public Guid ProgramId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid CourseResultId { get; set; }
        public Guid UnitResultId { get; set; }
        public EnumResultStatus? Status { get; set; }
        public double? Percent { get; set; }
        public int? TotalVideos { get; set; }
        public int? CompletedVideos { get; set; }

        [Column("TotalHomeworks")]
        public int? TotalHomeWork { get; set; }

        [Column("CompletedHomeworks")]
        public int? CompletedHomeWork { get; set; }

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
