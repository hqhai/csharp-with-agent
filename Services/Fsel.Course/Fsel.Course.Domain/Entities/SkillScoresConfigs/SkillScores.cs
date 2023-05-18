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
        public double Scores { get; set; }

        [JsonRequired]
        public double Total { get; set; }

        [JsonRequired]
        public double Number { get; set; }
    }
}
