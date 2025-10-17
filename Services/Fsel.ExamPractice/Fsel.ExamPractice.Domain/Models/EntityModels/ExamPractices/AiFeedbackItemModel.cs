// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Shared.Models.ShareModels;

    public class AiFeedbackItemModel
    {
        public string? Criteria { get; set; }
        public double BandScore { get; set; }
        public IList<ExamPracticeAIGradingLanguageModel>? GradingAlFeedbacks { get; set; }
    }
}
