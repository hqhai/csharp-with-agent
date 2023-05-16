// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using Fsel.Shared.Enums;

    public class SkillScores
    {
        public EnumCourseSkill Skill { get; set; }
        public long Scores { get; set; }
        public long Total { get; set; }
    }
}
