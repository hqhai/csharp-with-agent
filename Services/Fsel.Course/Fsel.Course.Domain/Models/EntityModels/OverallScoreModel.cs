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
        public EnumCourseType CourseType { get; set; }
    }
}
