// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class RequestSchemaAIModel : BaseRequestAIModel
    {
        [AliasAs("input")]
        [JsonPropertyName("input")]
        public IList<object>? Input { get; set; }

        [AliasAs("text")]
        [JsonPropertyName("text")]
        public object? Text { get; set; }
    }
}
