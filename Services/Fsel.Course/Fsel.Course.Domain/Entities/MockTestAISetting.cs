// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema; 
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class MockTestAISetting : Entity
    {
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }

        public EnumMockTestAIType Criteria { get; set; }


        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingModel { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTemperature { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingWordMaxLength { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTopP { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingFrequecy { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingPresence { get; set; }

        public Guid? SectionId { get; set; }

        public string? Task { get; set; }

        public string? PromptStr { get; set; }

        [NotMapped]
        public IList<MockTestPromptModel>? Prompts
        {
            get
            {
                return ConvertHelper.Deserialize<IList<MockTestPromptModel>>(PromptStr);
            }
            set { PromptStr = ConvertHelper.Serialize(value); }
        }

        public Section? Section { get; set; }
    }
}
