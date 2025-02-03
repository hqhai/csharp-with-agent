// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class ReportCourseLevelPTModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public double Percent { get; set; }
    }
}
