// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers
{
    public class ExamPracticeAnswerResponseModel
    {
        public Guid ExamPracticeSectionId { get; set; }
        public Guid ExamPracticeSectionResultId { get; set; }
        public string? WordContent { get; set; }
        public bool IsRetry { get; set; }
    }

    public class ExamPracticeAIGradingModel
    {
        public string? BandScore { get; set; }
        public string? BandDescriptorText { get; set; }
    }
}
