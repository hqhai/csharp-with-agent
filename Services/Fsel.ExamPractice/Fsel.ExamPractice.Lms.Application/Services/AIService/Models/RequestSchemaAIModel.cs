// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class RequestSchemaAIModel
    {
        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("input")]
        [JsonPropertyName("input")]
        public IList<object>? Input { get; set; }

        [AliasAs("text")]
        [JsonPropertyName("text")]
        public object? Text { get; set; }

        [AliasAs("top_p")]
        [JsonPropertyName("top_p")]
        public double TopP { get; set; }
    }
}
