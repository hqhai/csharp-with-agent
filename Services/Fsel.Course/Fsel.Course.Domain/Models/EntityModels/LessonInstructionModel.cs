// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;

    public class LessonInstructionModel : BaseModel
    {
        public string? Instruction { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
    }
}
