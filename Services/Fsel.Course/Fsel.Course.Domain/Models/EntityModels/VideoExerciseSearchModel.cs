// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class VideoExerciseSearchModel
    {
        public EnumCourseSkill CourseSkill { get; set; }
        public int Count { get; set; }
    }
}
