// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UserCourseSetting : Entity
    {
        public EnumUserCourseType Type { get; set; }
        public int Value { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public virtual User? User { get; set; }
        public Guid UserId { get; set; }
    }
}
