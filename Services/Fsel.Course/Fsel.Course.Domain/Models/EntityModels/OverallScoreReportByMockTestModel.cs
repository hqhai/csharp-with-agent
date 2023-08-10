// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class OverallScoreReportByMockTestModel : BaseModel
    {
        public string? Name { get; set; }
        public double Percent { get; set; }
        public IList<OverallScoreReportSkillModel>? OverallScoreReportSkills { get; set; }
    }

    public class OverallScoreReportSkillModel
    {
        public Guid? Id { get; set; }
        public double Percent { get; set; }
        public EnumResultStatus Status { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
