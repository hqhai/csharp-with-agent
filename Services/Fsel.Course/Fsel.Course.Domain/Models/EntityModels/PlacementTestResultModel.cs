// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class PlacementTestResultModel : BaseModel
    {
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public double Percent { get; set; }
        public int CorrectCount { get; set; }

        public double? OverallScore
        { get { return SkillScores != null && SkillScores.Count > 0 ? NumberHelper.RoundNumberDouble(SkillScores.Select(x => x.Scores).Average()) : default; } }

        public int CorrectTotal { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public bool IsLock { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
    }
}
