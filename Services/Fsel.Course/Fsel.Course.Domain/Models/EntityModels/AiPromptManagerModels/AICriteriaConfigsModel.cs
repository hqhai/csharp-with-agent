// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class AICriteriaConfigsModel : BaseModel
    {
        public Guid AiPromptManagerId { get; set; }
        public EnumCriteriaAi TypeCriteriaAi { get; set; }
        public string? UserRole { get; set; }
        public string? SettingAiConfig { get; set; }
        public string? SettingAiJson { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }
}
