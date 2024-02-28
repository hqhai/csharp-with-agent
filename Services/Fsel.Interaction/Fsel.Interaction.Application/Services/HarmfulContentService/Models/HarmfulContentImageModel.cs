// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService.Models
{
    using System.Text.Json.Serialization;

    public class HarmfulContentImageModel
    {
        [JsonPropertyName("Result")]
        public bool Result { get; set; }
    }
}
