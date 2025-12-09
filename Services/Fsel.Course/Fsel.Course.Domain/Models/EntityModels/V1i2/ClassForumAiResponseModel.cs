// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using System.Text.Json.Serialization;
    using Shared.Enums;

    public class ClassForumAiResponseModel
    {
        [JsonPropertyName("successCriteriaItem")]
        public string? SuccessCriteriaItem { get; set; }

        [JsonPropertyName("successCriteriaItemDetermination")]
        public EnumSuccessCriteriaItemDetermination SuccessCriteriaItemDetermination { get; set; }

        [JsonPropertyName("details")]
        public SuccessCriteriaDetails? Details { get; set; }

        [JsonPropertyName("criterionLevelJustification")]
        public string? CriterionLevelJustification { get; set; }

        [JsonPropertyName("criterionImprovementSuggestions")]
        public string? CriterionImprovementSuggestions { get; set; }
    }

    public class SuccessCriteriaDetails
    {
        [JsonPropertyName("successCriteriaItemEvidenceSet")]
        public List<SuccessCriteriaItemEvidenceSet>? SuccessCriteriaItemEvidenceSet { get; set; }
    }

    public class SuccessCriteriaItemEvidenceSet
    {
        [JsonPropertyName("successCriteriaItemEvidence")]
        public string? SuccessCriteriaItemEvidence { get; set; }

        [JsonPropertyName("successCriteriaItemFix")]
        public string? SuccessCriteriaItemFix { get; set; }

        [JsonPropertyName("explanationAndGuidance")]
        public string? ExplanationAndGuidance { get; set; }
    }
}
