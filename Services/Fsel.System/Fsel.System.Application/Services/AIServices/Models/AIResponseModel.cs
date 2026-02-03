// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.AIServices.Models
{
    using global::System.Text.Json.Serialization;
    using Refit;

    public class AIResponseModel
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

        [AliasAs("created")]
        [JsonPropertyName("created")]
        public long? Created { get; set; }

        [AliasAs("usage")]
        [JsonPropertyName("usage")]
        public object? Usage { get; set; }

        [AliasAs("choices")]
        [JsonPropertyName("choices")]
        public IList<Choice>? Choices { get; set; }

    }
    public class Choice
    {
        [AliasAs("message")]
        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [AliasAs("finish_reason")]
        [JsonPropertyName("finish_reason")]
        public string? FinishReson { get; set; }
        [AliasAs("index")]
        [JsonPropertyName("index")]
        public long Index { get; set; }
    }

    public class Message
    {
        [AliasAs("content")]
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        [AliasAs("role")]
        [JsonPropertyName("role")]
        public string? Role { get; set; }


    }
}
