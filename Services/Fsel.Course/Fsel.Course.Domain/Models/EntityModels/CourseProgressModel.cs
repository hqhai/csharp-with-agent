// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Ordering.Domain.Enums;

    public class CourseProgressModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public IList<CourseStudentProgressModel>? CourseStudentProgressModels { get; set; }
    }

    public class CourseStudentProgressModel
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid ClassId { get; set; }
        public string? CodeClass { get; set; }
        public Guid PackageId { get; set; }
        public EnumPackageCode PackageCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long TimeSpent { get; set; }
        public string? ContentCompleted { get; set; }
    }
}
