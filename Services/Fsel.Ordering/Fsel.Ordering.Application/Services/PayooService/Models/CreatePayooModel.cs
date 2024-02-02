// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService.Models
{
    using Newtonsoft.Json;

    public class CreatePayooModel
    {
        [JsonProperty("data")]
        public string? Data { get; set; }

        [JsonProperty("checksum")]
        public string? CheckSum { get; set; }
        [JsonProperty("refer")]
        public string? Refer { get; set; }
        [JsonProperty("method")]
        public string? Method { get; set; }
    }
}
