// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Enums.ErrorCodes;
    using Core.Entities;
    using Enums;
    using Fsel.Common.Helpers;

    public class AICriteriaConfigs : Entity
    {
        public Guid AiPromptManagerId { get; set; }
        public EnumCriteriaAi TypeCriteriaAi { get; set; }

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
