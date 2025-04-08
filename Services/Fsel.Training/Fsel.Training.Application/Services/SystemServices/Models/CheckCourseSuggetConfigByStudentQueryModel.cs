// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices.Models
{
    using Fsel.Shared.Enums;

    public class CheckCourseSuggetConfigByStudentQueryModel
    {
        public EnumCourseLevel BaseCourseLevel { get; set; }

        public EnumCourseLevel ChooseCourseLevel { get; set; }

        public int Age { get; set; }
    }
}
