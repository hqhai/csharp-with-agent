// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class StudentEventLearnProcessModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? District { get; set; }
        public string? School { get; set; }
        public string? SchoolClass { get; set; }
        public string? LevelCompletetion { get; set; }
        public string? CourseLevel { get; set; }
        public int? NumberOfMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? ContentProgress { get; set; }
        public double PercentProgress { get; set; }
        public int TotalLessonCompleted { get; set; }
        public IList<LearningProgressLearnModel> LearningProgressLearns { get; set; } = new List<LearningProgressLearnModel>();
    }
}
