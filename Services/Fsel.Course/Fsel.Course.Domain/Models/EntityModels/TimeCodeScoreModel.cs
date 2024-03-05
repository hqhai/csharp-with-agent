// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class TimeCodeScoreModel : VideoSkillScores
    {
        public double Percent { get; set; }
    }
}
