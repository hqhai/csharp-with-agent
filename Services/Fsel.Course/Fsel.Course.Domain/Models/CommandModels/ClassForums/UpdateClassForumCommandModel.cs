// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Course.Domain.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums
{
    public class UpdateClassForumCommandModel : BaseCommandModel
    {
        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }

        public long TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public Guid LessonId { get; set; }
    }
}
