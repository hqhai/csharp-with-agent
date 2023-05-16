// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class SkillScores
    {
        [JsonRequired]
        public EnumCourseSkill Skill { get; set; }

        [JsonRequired]
        public long Scores { get; set; }

        [JsonRequired]
        public long Total { get; set; }
    }
}
