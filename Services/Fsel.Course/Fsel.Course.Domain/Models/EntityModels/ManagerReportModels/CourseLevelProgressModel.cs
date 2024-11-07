// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Shared.Enums;

    public class CourseLevelProgressModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public int TotalCount { get; set; }
    }
}
