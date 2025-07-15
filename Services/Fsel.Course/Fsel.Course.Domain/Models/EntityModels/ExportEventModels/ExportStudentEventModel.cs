// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ExportEventModels
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ExportStudentEventModel
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public double NumberOfCompletedLessons { get; set; }
        public double TargetLessonCompletionRate { get; set; }
        public double ScoreLevelLesson { get; set; }
        public double? PercentComplete { get; set; }
        public int? TotalComplete { get; set; }
        public double? CountComplete { get; set; }
        public double? LevelCompletionRate { get; set; }
        public double? AchievedScore { get; set; }
        public double? AssignmentClassForum { get; set; }
        public double? ScoreLevelClassForum { get; set; }
        public double? NumberofCommentsonPosts { get; set; }
        public double? ScoreLevelComment { get; set; }
        public double? TotalScore { get; set; }
        public string? LocalId { get; set; }
        public Guid? GlobalId { get; set; }
        public string? EventCode { get; set; }
    }
}
