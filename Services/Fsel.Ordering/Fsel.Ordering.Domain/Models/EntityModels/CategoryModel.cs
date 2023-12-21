// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class CategoryModel
    {
        [JsonPropertyName("done")]
        public long Done { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("microtime")]
        public string? Microtime { get; set; }

        [JsonPropertyName("status")]
        public long Status { get; set; }

        [JsonPropertyName("data")]
        public IList<DataCategory>? Data { get; set; }
    }

    public partial class DataCategory
    {
        [JsonPropertyName("images")]
        public string? Images { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
