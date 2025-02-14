// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.AIService.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Refit;

    public class RequestAIModel
    {
        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("messages")]
        [JsonPropertyName("messages")]
        public IList<object>? Messages { get; set; }
    }
}
