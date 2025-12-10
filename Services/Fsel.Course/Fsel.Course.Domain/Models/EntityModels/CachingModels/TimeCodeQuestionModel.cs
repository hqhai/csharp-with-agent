// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.CachingModels
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public class TimeCodeQuestionModel
    {
        public Guid Id { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public Question? Question { get; set; }
        public Guid ExerciseId { get; set; }
        public SkillViewModel? Skill { get; set; }
    }
}
