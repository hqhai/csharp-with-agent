// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeGroupModel
    {
        public Guid Id { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeStatus ExamPracticeStatus { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsNew { get; set; }
        public double? ExecutionTime { get; set; }
        public int TotalSection { get; set; }
        public int TotalQuestion { get; set; }
        public int ParticipantCount { get; set; }
        public int TotalRetry { get; set; }
        public Guid? ExamPracticeResultId { get; set; }
        public EnumPracticeMode? PracticeMode { get; set; }
        public ExerciseConfig? Config { get; set; }
        public double? Score { get; set; }
        public int? CorrectCount { get; set; }
        public int? CorrectTotal { get; set; }
        public double? ExamPracticeScore { get; set; }
        public EnumResultStatus? Status { get; set; }
        public double? ProgressPercent { get; set; }
        public string? ScoreLevel { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public IList<EnumCourseSkill>? RemainingSkills { get; set; }
    }
}
