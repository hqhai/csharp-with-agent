// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseTargetConfigs
{
    using Fsel.Shared.Enums;

    public class CreateCourseTargetConfigCommandModel
    {
        public string? Title { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public int LessonNumberPerWeek { get; set; }

        public int MaxHoursPerLesson { get; set; }
    }
}
