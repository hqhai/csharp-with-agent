// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonInstructions
{
    using Fsel.Common.Enums;

    public class UpdateLessonInstructionCommandModel
    {
        public Guid Id { get; set; }
        public string? Instruction { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}
