// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("StudentWeeklyLearningProgress", Schema = "dbo")]
    public class StudentWeeklyLearningProgress
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseResultId { get; set; }
        public Guid ProgramId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int WeekIndex { get; set; }
        public int WeeklyTargetLessons { get; set; }
        public int CompletedLessons { get; set; }
        public EnumCurrentProgressStatus ProgressStatus { get; set; }
    }
}
