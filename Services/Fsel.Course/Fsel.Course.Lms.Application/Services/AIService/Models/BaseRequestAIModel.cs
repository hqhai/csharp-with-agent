// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    /// <summary>
    /// Base class cho các AI request model - chứa các properties chung
    /// </summary>
    public abstract class BaseRequestAIModel
    {
        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("temperature")]
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [AliasAs("top_p")]
        [JsonPropertyName("top_p")]
        public double TopP { get; set; }
    }
}
