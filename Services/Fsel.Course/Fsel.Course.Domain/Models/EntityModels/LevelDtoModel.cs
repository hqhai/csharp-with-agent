// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class LevelDtoModel
    {
        public EnumSkillLevel? SkillLevel { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? LevelName { get; set; }
        public EnumResultStatus? Status { get; set; }
        public bool? IsResetCourse { get; set; }
        public bool IsUsedLevel { get; set; }
        public bool IsHiddenCourseLevel { get; set; } = true;
    }
}
