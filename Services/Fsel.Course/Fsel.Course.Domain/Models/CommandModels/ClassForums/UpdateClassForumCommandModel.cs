// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums
{
    public class UpdateClassForumCommandModel : BaseCommandModel
    {
        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }

        public double TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public Guid LessonId { get; set; }

        public IList<string>? FilePaths { get; set; }
    }
}
