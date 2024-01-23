// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;

    public class CategoriesAnalysisModel
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("severity")]
        public long Severity { get; set; }
    }
}
