// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentProgressModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel Level { get; set; }
        public string? Subject { get; set; }
        public string? ContentProgress { get; set; }
        public int? DisplayOrderUnit { get; set; }
        public int? DisplayOrderLesson { get; set; }
        public string? Program { get; set; }
    }
}
