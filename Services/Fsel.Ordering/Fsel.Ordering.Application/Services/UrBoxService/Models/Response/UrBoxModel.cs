// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using System.Text.Json.Serialization;

    public class UrBoxModel
    {
        [JsonPropertyName("done")]
        public long Done { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("microtime")]
        public string? MicroTime { get; set; }

        [JsonPropertyName("status")]
        public long Status { get; set; }
    }
}
