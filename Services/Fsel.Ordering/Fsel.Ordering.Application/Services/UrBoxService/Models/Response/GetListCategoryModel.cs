// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GetListCategoryModel : UrBoxModel
    {
        [JsonPropertyName("data")]
        public IList<CategoryModel>? Data { get; set; }
    }

    public partial class CategoryModel
    {
        [JsonPropertyName("images")]
        public string? Images { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
