// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeScoreModel
    {
        public EnumExamPracticeScoreCriteria Criteria { get; set; }
        public string? FeedBack { get; set; }
        public string? GradingAlFeedback { get; set; }
        public long Score { get; set; }
    }
}
