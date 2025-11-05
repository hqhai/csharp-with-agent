// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentDetailModel
    {
        public Guid Id { get; set; }
        public string? School { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? SchoolClassId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
