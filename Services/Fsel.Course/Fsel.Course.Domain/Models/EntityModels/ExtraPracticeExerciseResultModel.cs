// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class ExtraPracticeExerciseResultModel : BaseResultModel
    {
        public int ExecuteCount { get; set; }
        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public Guid ExtraPracticeExerciseId { get; set; }
    }
}
