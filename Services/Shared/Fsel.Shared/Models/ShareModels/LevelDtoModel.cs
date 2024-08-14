// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class LevelDtoModel
    {
        public EnumSkillLevel? SkillLevel { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? LevelName { get; set; }
    }
}
