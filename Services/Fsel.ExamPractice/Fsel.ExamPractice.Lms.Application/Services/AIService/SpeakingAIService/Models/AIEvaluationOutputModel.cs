// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class AIEvaluationOutputModel
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
