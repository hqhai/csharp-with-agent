// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class VideoExcerciseSearchModel
    {
        public EnumCourseSkill CourseSkill { get; set; }
        public int Count { get; set; }
    }
}
