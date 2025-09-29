// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers
{
    using System.Text.Json.Serialization;

    public class ExamPracticeAnswerResponseModel
    {
        public Guid ExamPracticeSectionId { get; set; }
        public Guid ExamPracticeSectionResultId { get; set; }
        public string? WordContent { get; set; }
        public bool IsRetry { get; set; }
    }

    public class ExamPracticeAIGradingDataModel
    {
        [JsonPropertyName("band_score")]
        public double? BandScore { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [JsonPropertyName("suggestions_for_improvement")]
        public string? SuggestionsForImprovement { get; set; }
    }
}
