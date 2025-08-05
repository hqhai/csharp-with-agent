// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Entities.BandScoresConfigs;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeSectionResultModel : BaseModel
    {
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public virtual double Percent { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public int? HighestStreak { get; set; }
        public double WorkingTime { get; set; }
        public double RemainingTime { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public Guid ExamPracticeResultId { get; set; }
        public bool IsTestDone { get; set; }
        public BandScoresReport? BandScoresReport { get; set; }
        public double TargetBandScore { get; set; }
        public bool IsCheckScoreColor { get; set; }
        public bool IsFeedBack { get; set; }
        public Guid? ParentExamPracticeSectionResultId { get; set; }
    }
}
