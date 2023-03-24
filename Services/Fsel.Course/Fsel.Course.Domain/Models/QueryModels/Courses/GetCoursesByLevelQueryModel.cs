// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    using Fsel.Common.Enums;

    public class GetCoursesByLevelQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
