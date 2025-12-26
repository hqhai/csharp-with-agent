// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class ResponsesAIModel
    {
        [AliasAs("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [AliasAs("object")]
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("created_at")]
        [JsonPropertyName("created_at")]
        public long? Created { get; set; }

        [AliasAs("output")]
        [JsonPropertyName("output")]
        public IList<Output>? Output { get; set; }
    }

    public class Output
    {
        [AliasAs("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [AliasAs("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [AliasAs("status")]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [AliasAs("role")]
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [AliasAs("content")]
        [JsonPropertyName("content")]
        public IList<Content>? Content { get; set; }
    }

    public class Content
    {
        [AliasAs("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [AliasAs("text")]
        [JsonPropertyName("text")]  //refusal
        public string? Text { get; set; }
    }
}
