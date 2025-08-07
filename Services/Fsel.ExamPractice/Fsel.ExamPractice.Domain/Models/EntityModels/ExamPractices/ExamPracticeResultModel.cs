// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeResultModel : BaseModel
    {
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public virtual double Percent { get; set; }
        public IList<SkillScores>? SkillScores { get; set; } = new List<SkillScores>();
        public EnumPracticeMode? PracticeMode { get; set; }
        public ExerciseConfig? Config { get; set; }
        public int? HighestStreak { get; set; }
        public double WorkingTime { get; set; }
        public double RemainingTime { get; set; }
        public EnumResultStatus Status { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public Guid StudentId { get; set; }
        public int ResultPosition { get; set; }
        public Guid ExamPracticeRetryId { get; set; }
        public Guid ExamPracticeId { get; set; }
    }
}
