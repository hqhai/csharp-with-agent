// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class PlacementTestResultModel : BaseResultScoreModel
    {
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }

        public double? OverallScore
        { get { return SkillScores != null && SkillScores.Count > 0 ? NumberHelper.RoundNumberDouble(SkillScores.Select(x => x.Scores).Average()) : default; } }

        public EnumPlacementTestLevel Level { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public bool IsLock { get; set; }
    }
}
