// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService.Models
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Models.ShareModels;

    public class HarmfulContentWordsModel
    {
        [JsonPropertyName("categoriesAnalysis")]
        public IList<CategoriesAnalysisModel>? CategoriesAnalysis { get; set; }
    }
}
