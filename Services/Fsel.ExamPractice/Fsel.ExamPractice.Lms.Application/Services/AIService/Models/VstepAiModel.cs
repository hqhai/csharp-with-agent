// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class VstepAiModel
    {
        [AliasAs("bandscore")]
        [JsonPropertyName("bandscore")]
        public double BandScore { get; set; }

        [AliasAs("explanation")]
        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [AliasAs("suggestions_for_improvement")]
        [JsonPropertyName("suggestions_for_improvement")]
        public string? SuggestionsForImprovement { get; set; }
    }
}
