// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AiService.Models
{
    using System.Text.Json.Serialization;
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
        public IList<object>? Choices { get; set; }
    }
}
