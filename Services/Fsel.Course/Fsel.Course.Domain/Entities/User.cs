// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class User : Entity
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<EnumCourseType>? Types { get; set; }
        public IList<EnumTeacherRole>? TeacherRoles { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public EnumRoleRegisterWithAdmin Role { get; set; }
    }
}
