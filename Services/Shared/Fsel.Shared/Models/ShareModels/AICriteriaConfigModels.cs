// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    /// <summary>
    /// Shared AI Criteria Config model for both Course and System services
    /// </summary>
    public class SharedAICriteriaConfigModel
    {
        /// <summary>
        /// Trạng thái version (LastVersion, OldVersion)
        /// </summary>
        public string? VersionStatus { get; set; }

        /// <summary>
        /// Số phiên bản
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// ID của entity gốc (null cho version đầu tiên)
        /// </summary>
        public string? OriginalId { get; set; }

        /// <summary>
        /// Loại version (V1, V2, V3)
        /// </summary>
        public string? VersionType { get; set; }

        public string? AiPromptManagerId { get; set; }

        public string? AiModel { get; set; }

        public string? SchemaType { get; set; }

        public string? SchemaName { get; set; }

        public List<SharedAiCriteriaModel>? AiCriteriaModels { get; set; }

        public double? SettingTemperature { get; set; }

        public double? SettingWordMaxLength { get; set; }

        public double? SettingTopP { get; set; }

        public double? SettingFrequency { get; set; }

        public double? SettingPresence { get; set; }

        public int? MaximumNumber { get; set; }

        public int? MaximumToken { get; set; }
    }

    public class SharedAiCriteriaModel
    {
        public string? FeatureMultiple { get; set; }

        public string? SubFeatureType { get; set; }

        public string? TypeCriteriaAi { get; set; }

        public string? UserRole { get; set; }

        public string? ObjectId { get; set; }

        public string? SettingAiConfig { get; set; }

        public string? SettingAiJson { get; set; }
    }

    /// <summary>
    /// AI Configuration wrapper for Semantic Dictionary
    /// </summary>
    public class SharedAICriteriaConfig
    {
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public double? TopP { get; set; }
        public double? FrequencyPenalty { get; set; }
        public double? PresencePenalty { get; set; }
        public string? SystemRole { get; set; }
        public string? UserRole { get; set; }
    }
}
