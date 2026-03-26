// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("StudentLearningProgress", Schema = "dbo")]
    public class StudentLearningProgress
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseResultId { get; set; }
        public Guid ProgramId { get; set; }
        public int TotalCompletedLessons { get; set; }
        public int TotalTargetLessons { get; set; }
        public EnumCurrentProgressStatus CurrentProgressStatus { get; set; }
        public DateTime LearningStartDate { get; set; }
        public DateTime? LastCompletedLessonDate { get; set; }
        public int TotalLearningWeeks { get; set; }
        public int AverageCompletedLessonsPerWeek { get; set; }
    }

    public enum EnumCurrentProgressStatus
    {
        BelowTarget,
        OnTarget,
        AboveTarget
    }
}
