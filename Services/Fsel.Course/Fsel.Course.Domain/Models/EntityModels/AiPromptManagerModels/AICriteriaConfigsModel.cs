// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Core.Base.BaseModels;
    using Enums;

    public class AICriteriaConfigsModel
    {
        public Guid AiPromptManagerId { get; set; }
        public IList<AiCriteriaModel> AiCriteriaModel { get; set; }
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
        public string? SettingAiConfig { get; set; }
        public string? SettingAiJson { get; set; }
    }
}
