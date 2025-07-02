// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateStudentCommandModel
    {
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid ClassId { get; set; }
    }
}
