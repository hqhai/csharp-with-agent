// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonInstructions
{
    public class CreateLessonInstructionCommandModel
    {
        public string? Instruction { get; set; }

        public Guid? SkillId { get; set; }
    }
}
