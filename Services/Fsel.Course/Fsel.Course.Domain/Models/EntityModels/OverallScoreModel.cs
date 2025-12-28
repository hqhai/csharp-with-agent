// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Shared.Enums;

    public class OverallScoreModel
    {
        public IList<SkillScores>? SkillScores { get; set; }
        public double Percent { get; set; }
        public bool IsPlacement { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? ProgramId { get; set; }
        public string? ProgramName { get; set; }

        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel NextCourseLevel { get; set; }
        public Guid? NextLevelId { get; set; }
        public string? NextLevelName { get; set; }
        public double TargetBandScores { get; set; }
        public double BandScores { get; set; }
    }
}
