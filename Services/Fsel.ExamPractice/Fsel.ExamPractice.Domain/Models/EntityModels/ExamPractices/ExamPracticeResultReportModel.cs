// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeResultReportModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public EnumPracticeMode? PracticeMode { get; set; }
        public ExerciseConfig? Config { get; set; }
        public double? ExamPracticeScore { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public double RemainingTime { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public int? HighestStreak { get; set; }
        public double WorkingTime { get; set; }
        public double? Score { get; set; }
        public bool? IsTeacherGraded { get; set; }
        public double TargetBandScore { get; set; }
        public bool IsCheckScoreColor { get; set; }
    }
}
