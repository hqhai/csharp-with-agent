// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonInstructions
{
    using Fsel.Shared.Enums;

    public class CreateLessonInstructionCommandModel
    {
        public Guid? Id { get; set; }

        public string? Instruction { get; set; }

        public Guid? SkillId { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
    }
}
