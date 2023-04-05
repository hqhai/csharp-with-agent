// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorks
{
    public class CreateHomeWorkCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? MediaPost { get; set; }
        public bool IsActive { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}
