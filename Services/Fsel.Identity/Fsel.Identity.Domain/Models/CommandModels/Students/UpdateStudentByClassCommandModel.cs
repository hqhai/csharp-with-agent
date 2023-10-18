// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using Fsel.Shared.Enums;

    public class UpdateStudentByClassCommandModel
    {
        public Guid ClassId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? PackageId { get; set; }
        public int NumberOfShield { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
