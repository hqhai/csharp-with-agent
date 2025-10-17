// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class ExamPracticeScoreModel
    {
        public EnumExamPracticeScoreCriteria Criteria { get; set; }
        public string? FeedBack { get; set; }
        public long Score { get; set; }
        public IList<ExamPracticeAIGradingLanguageModel> GradingAlFeedbacks { get; set; } = new List<ExamPracticeAIGradingLanguageModel>();
    }
}
