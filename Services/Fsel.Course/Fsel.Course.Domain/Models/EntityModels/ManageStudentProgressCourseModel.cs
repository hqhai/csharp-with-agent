// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class ManageStudentProgressCourseModel
    {
        public string? FullName { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? ContentProgress { get; set; }
        public int DisplayOrderUnit { get; set; }
        public int DisplayOrderLesson { get; set; }
    }
}
