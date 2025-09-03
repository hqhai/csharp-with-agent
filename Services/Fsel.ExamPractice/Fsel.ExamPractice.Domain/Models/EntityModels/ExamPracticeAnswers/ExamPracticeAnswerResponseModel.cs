// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers
{
    using System.Text.Json.Serialization;
    using Refit;

    public class ExamPracticeAnswerResponseModel
    {
        public Guid ExamPracticeSectionId { get; set; }
        public Guid ExamPracticeSectionResultId { get; set; }
        public string? WordContent { get; set; }
        public bool IsRetry { get; set; }
    }

    public class ExamPracticeAIGradingLanguageModel
    {
        public string? Language { get; set; }
        public IList<ExamPracticeAIGradingModel>? ExamPracticeAIGradings { get; set; }
    }

    public class ExamPracticeAIGradingModel
    {
        public string? BandScore { get; set; }
        public string? BandDescriptorText { get; set; }

        [AliasAs("explanation")]
        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [AliasAs("suggestions_for_improvement")]
        [JsonPropertyName("suggestions_for_improvement")]
        public string? SuggestionsForImprovement { get; set; }
    }
}
