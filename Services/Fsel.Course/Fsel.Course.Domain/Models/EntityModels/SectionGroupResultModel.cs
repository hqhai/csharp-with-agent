// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class SectionGroupResultModel : BaseModel
    {
        public double? Percent { get; set; }
        public EnumResultStatus Status { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public double CorrectCount { get; set; }
        public double CorrectTotal { get; set; }
        public Guid SectionGroupId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public Guid? FinalTestResultId { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
    }
}
