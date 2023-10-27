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
        public double TotalCount { get; set; }

        [JsonRequired]
        public double CorrectCount { get; set; }

        [JsonRequired]
        public double TotalQuestion { get; set; }

        [JsonRequired]
        public double CountQuestion { get; set; }

        [JsonRequired]
        public double Percent { get; set; }
    }
}
