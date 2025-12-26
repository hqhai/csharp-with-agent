// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Enums.ErrorCodes;
    using Common.Helpers;
    using Core.Entities;
    using Enums;
    using Fsel.Common.Enums;
    using Fsel.Shared.Enums;

    public class AICriteriaConfigs : Entity
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
        public EnumVersion VersionType { get; set; } = EnumVersion.V2;

        public Guid AiPromptManagerId { get; set; }

        public EnumFeatureMultiple? FeatureMultiple { get; set; }

        public EnumSubFeatureType? SubFeatureType { get; set; }

        public EnumCriteriaAi? TypeCriteriaAi { get; set; }

        public EnumDefaultType? DefaultType { get; set; }

        public Guid ProjectId { get; set; }

        public Guid? ObjectId { get; set; }

        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UserRole { get; set; }

        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingAiConfig { get; set; }

        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingAiJson { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? SettingTemperature { get; set; }

        [Range(0, 4095, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? SettingWordMaxLength { get; set; }

        [Range(0, 1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? SettingTopP { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? SettingFrequency { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? SettingPresence { get; set; }

        [Range(1, 10, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? MaximumNumber { get; set; }

        [Range(1, 10000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? MaximumToken { get; set; } = 4000;

        public AiPromptManager? AiPromptManager { get; set; }

        [NotMapped]
        public object? JsonConfig
        {
            get { return ConvertHelper.Deserialize<object>(SettingAiJson); }
            set { SettingAiJson = ConvertHelper.Serialize(value); }
        }
    }
}
