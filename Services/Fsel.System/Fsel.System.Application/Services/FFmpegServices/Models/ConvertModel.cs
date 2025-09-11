// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.FFmpegServices.Models
{
    using global::System.Text.Json.Serialization;

    public class ConvertModel
    {
        [JsonPropertyName("path_s3")]
        public string? Paths3 { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
