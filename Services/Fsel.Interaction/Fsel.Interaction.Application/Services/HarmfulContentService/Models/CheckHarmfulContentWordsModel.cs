// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService.Models
{
    using System.Text.Json.Serialization;

    public class CheckHarmfulContentWordsModel
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("categories")]
        public IList<string>? Categories { get; set; }

        [JsonPropertyName("outputType")]
        public string? OutputType { get; set; }
    }
}
