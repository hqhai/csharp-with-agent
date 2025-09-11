// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;

    public class AiFeedbackItemModel
    {
        public string? Criteria { get; set; }
        public IList<ExamPracticeAIGradingLanguageModel>? GradingAlFeedback { get; set; }
    }
}
