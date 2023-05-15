// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UnitSkillScoreModel
    {
        public EnumCourseSkill Skill { get; set; }
        public long TotalCount { get; set; }
        public long CorrectCount { get; set; }
    }
}
