// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ExportEventModels
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ExportSchoolEventModel
    {
        public string? School { get; set; }
        public double NumberOfCompletedLessons { get; set; }
        public double TargetLessonCompletionRate { get; set; }
        public double ScoreLevelLesson { get; set; }
        public double? LevelCompletionRate { get; set; }
        public double? AchievedScore { get; set; }
        public double? AssignmentClassForum { get; set; }
        public double? ScoreLevelClassForum { get; set; }
        public double? NumberofCommentsonPosts { get; set; }
        public double? ScoreLevelComment { get; set; }
        public double? TotalScore { get; set; }
    }
}
