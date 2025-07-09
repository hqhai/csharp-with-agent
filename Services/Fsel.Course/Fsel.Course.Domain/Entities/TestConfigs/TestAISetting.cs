// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;

    public class TestAISetting : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingModel { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTemperature { get; set; }

        [Range(0, 4095, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingWordMaxLength { get; set; }

        [Range(0, 1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTopP { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingFrequecy { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingPresence { get; set; }

        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Task { get; set; }

        public string? PromptStr { get; set; }

        [NotMapped]
        public IList<TestPromptModel>? Prompts
        {
            get
            {
                return ConvertHelper.Deserialize<IList<TestPromptModel>>(PromptStr);
            }
            set { PromptStr = ConvertHelper.Serialize(value); }
        }

        public Guid TestSectionId { get; set; }
        public TestSection? TestSection { get; set; }
        public ICollection<TestAICriteriaSetting> TestAICriteriaSettings { get; set; } = new List<TestAICriteriaSetting>();
    }
}
