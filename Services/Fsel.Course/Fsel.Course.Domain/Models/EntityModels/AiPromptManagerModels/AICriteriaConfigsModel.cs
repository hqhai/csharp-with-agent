// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Core.Base.BaseModels;
    using Enums;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class AICriteriaConfigsModel
    {
        /// <summary>
        /// Trạng thái version (LastVersion, OldVersion)
        /// </summary>
        public EnumVersionStatus VersionStatus { get; set; }

        /// <summary>
        /// Số phiên bản
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// ID của entity gốc (null cho version đầu tiên)
        /// </summary>
        public Guid? OriginalId { get; set; }

        /// <summary>
        /// Loại version (V1, V2, V3)
        /// </summary>
        public EnumVersion VersionType { get; set; }

        public Guid AiPromptManagerId { get; set; }

        public string? SchemaType { get; set; }
        public string? SchemaName { get; set; }
        public IList<AiCriteriaModel>? AiCriteriaModels { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }

    public class AiCriteriaModel : BaseModel
    {
        public EnumFeatureMultiple? FeatureMultiple { get; set; }
        public EnumSubFeatureType? SubFeatureType { get; set; }
        public EnumCriteriaAi? TypeCriteriaAi { get; set; }
        public string? UserRole { get; set; }
        public Guid? ObjectId { get; set; }
        public string? SettingAiConfig { get; set; }
        public string? SettingAiJson { get; set; }
        public object? JsonConfig
        {
            get { return ConvertHelper.Deserialize<object>(SettingAiJson); }
            set
            {
                // If value is already a JSON string, assign directly to avoid double-encoding
                if (value is string jsonString)
                {
                    SettingAiJson = jsonString;
                }
                else
                {
                    SettingAiJson = ConvertHelper.Serialize(value);
                }
            }
        }
    }
}
