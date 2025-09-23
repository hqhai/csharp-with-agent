// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class HomeWorkExtraPracticeResultModel
    {
        public Guid Id { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public double Percent { get; set; }
        public Guid StudentId { get; set; }
        public EnumResultStatus Status { get; set; }
        public EnumSubmissionCount SubmissionCount { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
