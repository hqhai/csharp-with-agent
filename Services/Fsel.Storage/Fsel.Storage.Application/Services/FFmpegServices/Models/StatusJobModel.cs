// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Services.FFmpegServices.Models
{
    using System.Text.Json.Serialization;

    public class StatusJobModel
    {
        [JsonPropertyName("job_id")]
        public Guid JobId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("path_s3")]
        public string? Paths3 { get; set; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }

        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        [JsonPropertyName("file_data")]
        public string? FileData { get; set; }
    }
}
