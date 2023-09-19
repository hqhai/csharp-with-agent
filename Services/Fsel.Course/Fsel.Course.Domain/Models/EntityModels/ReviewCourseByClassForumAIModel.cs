// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ReviewCourseByClassForumAIModel
    {
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
        public Guid UnitId { get; set; }
        public Guid ClassForumId { get; set; }
        public string? CourseName { get; set; }
        public string? Code { get; set; }
        public double NumberOfStarts { get; set; }
        public int TotalRating { get; set; }
    }
}
