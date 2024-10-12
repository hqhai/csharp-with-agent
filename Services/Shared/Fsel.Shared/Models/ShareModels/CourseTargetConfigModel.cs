// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseTargetConfigModel : BaseModel
    {
        public string? Title { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public int LessonNumberPerWeek { get; set; }

        public int MaxHoursPerLesson { get; set; }

        public int? Month { get; set; }
    }
}
