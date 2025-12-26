// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.DashboardModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class CompetencyRadarModel
    {
        public string? Texti18n { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
