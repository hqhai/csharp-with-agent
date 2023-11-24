// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentCourseUnitModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public int UnitNumber { get; set; }
    }
}
