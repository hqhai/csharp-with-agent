// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Ordering.Domain.Enums;

    public class CourseManagerProgressModel
    {
        public string? FullName { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid ClassId { get; set; }
        public Guid PackageId { get; set; }
        public EnumPackageCode PackageCode { get; set; }
        public string? CodeClass { get; set; }
        public string? ContentCompleted { get; set; }
        public int TotalVisit { get; set; }
        public long TimeSpent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
