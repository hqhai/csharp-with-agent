// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentGoalSummaryModel : BaseModel
    {
        public int LessonsPerWeek { get; set; }
        public int CompletedLessons { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? LastCompletedAt { get; set; }
        public EnumProgressStatus ProgressStatus { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }
}
