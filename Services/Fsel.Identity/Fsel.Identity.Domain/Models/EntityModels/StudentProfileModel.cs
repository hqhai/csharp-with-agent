// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class StudentProfileModel
    {
        public Guid Id { get; set; }
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? CodeClass { get; set; }
        public string? School { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? UserName { get; set; }
    }
}
