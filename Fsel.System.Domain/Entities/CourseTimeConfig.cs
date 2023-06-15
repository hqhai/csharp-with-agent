// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CourseTimeConfig : Entity
    {
        public EnumCourseLevel CourseLevel { get; set; }

        public int DurationMonth { get; set; }

        public int EnrollmentWeek { get; set; }
    }
}
