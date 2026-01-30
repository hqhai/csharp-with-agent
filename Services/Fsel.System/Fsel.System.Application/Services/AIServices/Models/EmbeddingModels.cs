// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.AIServices.Models
{
    using global::System.Text.Json.Serialization;
    using Refit;

    /// <summary>
    /// Request model for embedding API
    /// </summary>
    public class EmbeddingRequest
    {
        [AliasAs("input")]
        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;

        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string Model { get; set; } = "text-embedding-3-small";
    }

    /// <summary>
    /// Response model from embedding API
    /// </summary>
    public class EmbeddingResponseModel
    {
        [AliasAs("object")]
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [AliasAs("data")]
        [JsonPropertyName("data")]
        public List<EmbeddingData>? Data { get; set; }

        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("usage")]
        [JsonPropertyName("usage")]
        public EmbeddingUsage? Usage { get; set; }
    }

    public class EmbeddingData
    {
        [AliasAs("object")]
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [AliasAs("index")]
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [AliasAs("embedding")]
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; } = new();
    }

    public class EmbeddingUsage
    {
        [AliasAs("prompt_tokens")]
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [AliasAs("total_tokens")]
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
