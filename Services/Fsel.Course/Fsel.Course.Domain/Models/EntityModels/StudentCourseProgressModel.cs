// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class StudentCourseProgressModel
    {
        public int CurrentLessonIndex { get; set; }
        public int TotalLessons { get; set; }
        public bool IsCourseCompleted { get; set; }
        public string? UnitName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastAccessedDate { get; set; }
        public long? TotalActiveDuration { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
