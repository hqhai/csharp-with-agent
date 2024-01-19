// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService.Models
{
    using System.Text.Json.Serialization;

    public class CheckHarmfulContentImagesModel
    {
        [JsonPropertyName("DataRepresentation")]
        public string? DataRepresentation { get; set; }
        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }
}
