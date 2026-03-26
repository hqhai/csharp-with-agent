// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("CourseResult_Report", Schema = "dbo")]
    public class CourseResult
    {
        public Guid CourseResultId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumResultStatus? Status { get; set; }
        public EnumWorkingStatus? WorkingStatus { get; set; }
        public decimal? Percent { get; set; }
        public string? SkillScoresJson { get; set; }
        public int? TotalUnits { get; set; }
        public int? CompletedUnits { get; set; }
        public int? TotalLessons { get; set; }
        public int? CompletedLessons { get; set; }
        public int? TotalTests { get; set; }
        public int? CompletedTests { get; set; }
        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
