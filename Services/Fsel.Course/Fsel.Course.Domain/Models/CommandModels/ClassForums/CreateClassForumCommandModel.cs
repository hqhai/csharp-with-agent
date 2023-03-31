// Copyright (c) Atlantic. All rights reserved.

using System.Text.Json.Serialization;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums
{
    public class CreateClassForumCommandModel
    {
        public string? Title { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }
        public long TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
    }
}
