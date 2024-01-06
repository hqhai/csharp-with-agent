// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService.Models
{
    using System.Text.Json.Serialization;

    public class CheckHarmfulContentImagesModel
    {
        [JsonPropertyName("image")]
        public ImageModel? Images { get; set; }
    }

    public class ImageModel
    {
        [JsonPropertyName("blobUrl")]
        public string? FilePath { get; set; }
    }
}
