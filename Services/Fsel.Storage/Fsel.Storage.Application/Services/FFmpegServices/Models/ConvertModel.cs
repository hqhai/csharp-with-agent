// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Services.FFmpegServices.Models
{
    using System.Text.Json.Serialization;

    public class ConvertModel
    {
        [JsonPropertyName("job_id")]
        public Guid? JobId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("input_file")]
        public string? InputFile { get; set; }

        [JsonPropertyName("path_s3")]
        public string? PathS3 { get; set; }
    }
}
