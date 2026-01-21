// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.AIServices.Models
{
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using global::System.Text.Json.Serialization;
    using Refit;

    public class RequestAIModel
    {
        [AliasAs("model")]
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [AliasAs("messages")]
        [JsonPropertyName("messages")]
        public IList<ChatBotMessageModel>? Messages { get; set; }

        [AliasAs("temperature")]
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [AliasAs("max_tokens")]
        [JsonPropertyName("max_tokens")]
        public double MaxTokens { get; set; }

        [AliasAs("top_p")]
        [JsonPropertyName("top_p")]
        public double TopP { get; set; }

        [AliasAs("frequency_penalty")]
        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; }

        [AliasAs("presence_penalty")]
        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; }
    }
}
