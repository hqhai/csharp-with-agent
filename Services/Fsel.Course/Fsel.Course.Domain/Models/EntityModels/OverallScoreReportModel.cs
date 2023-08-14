// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class OverallScoreReportModel
    {
        public double CountQuestion { get; set; }
        public double TotalQuestion { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
