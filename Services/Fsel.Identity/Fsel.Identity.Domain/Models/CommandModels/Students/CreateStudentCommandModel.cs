// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Identity.Domain.Entities;

    public class CreateStudentCommandModel
    {
        public string? Membership { get; set; }

        public string? Occupation { get; set; }
        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid ClassId { get; set; }

        public Guid HumanId { get; set; }
    }
}
