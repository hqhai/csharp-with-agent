// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class SectionGroupResultModel : BaseResultModel
    {
        public IList<SkillScores>? SkillScores { get; set; }
        public Guid SectionGroupId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public Guid? FinalTestResultId { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public double ExecutionTime { get; set; }
    }
}
