// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentGoalAggregateModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ClassName { get; set; }
        public int TotalCompletedLessons { get; set; }
        public int TotalTargetLessons { get; set; }
        public int LessonsPerWeek { get; set; }
        public int CompletedLessons { get; set; }
        public int ConsecutiveBehindWeeks { get; set; }
        public EnumCombinedProgress? CombinedProgress { get; set; }
        public EnumProgressStatus ProgressStatus { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumStatusStudentGoal StatusStudentGoal { get; set; }
        public string? StudentCampusCode { get; set; }
        public string? ClassCampusCode { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public Guid? UserId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }
}
