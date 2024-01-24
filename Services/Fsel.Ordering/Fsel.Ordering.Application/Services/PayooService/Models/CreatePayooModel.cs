// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class CreatePayooModel
    {
        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonProperty("checksum")]
        [JsonPropertyName("checksum")]
        public string? CheckSum { get; set; }
        [JsonProperty("refer")]
        [JsonPropertyName("refer")]
        public string? Refer { get; set; }
        [JsonProperty("method")]
        [JsonPropertyName("method")]
        public string? Method { get; set; }
        [JsonProperty("bank")]
        [JsonPropertyName("bank")]
        public string? Bank { get; set; }
    }
}
