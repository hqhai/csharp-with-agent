// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class CourseSuggestConfigStudentModel
    {
        public EnumCourseSuggestType Type { get; set; }

        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}
