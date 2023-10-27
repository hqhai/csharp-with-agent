// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using Fsel.Course.Domain.Enums;

    public class TestSkillScores : SkillScores
    {
        public EnumResultStatus? Status { get; set; }
        public double PercentProgress { get; set; }
    }
}
