// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Shared.Enums;

    public class OverallScoreReportModel : IHighestStreak
    {
        public double CountQuestion { get; set; }
        public double TotalQuestion { get; set; }
        public double CorrectCount { get; set; }
        public double CorrectTotal { get; set; }
        public long WorkingTime { get; set; }
        public int? HighestStreak { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public IList<SkillViewModel>? Skills { get; set; }
    }
}
