// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class LessonInstructionModel
    {
        public Guid Id { get; set; }
        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public string? Instruction { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}
