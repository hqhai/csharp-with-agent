// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FeedbackClassForumAIModel
    {
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
        public Guid UnitId { get; set; }
        public Guid ClassForumId { get; set; }
        public string? CourseName { get; set; }
        public string? Code { get; set; }
        public double NumberOfStarts { get; set; }
        public int TotalRating { get; set; }
        public int LessonDisplayOrder { get; set; }
        public int UnitDisplayOrder { get; set; }
    }
}
