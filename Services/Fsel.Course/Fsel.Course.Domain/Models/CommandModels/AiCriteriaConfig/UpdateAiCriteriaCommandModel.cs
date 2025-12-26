// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Enums;

    public class UpdateAiCriteriaCommandModel
    {
        public Guid Project { get; set; }
        public EnumFeatureMultiple? FeatureMultiple { get; set; }
        public EnumSubFeatureType? SubFeatureType { get; set; }
        public Guid AiPromptManagerId { get; set; }
        public UpdateAiCriteriaCommand? AiCriteriaConfig { get; set; }
        public IList<UpdateAiCriteriaCommand>? AiCriteriaConfigs { get; set; }
    }
    public class UpdateAiCriteriaCommand
    {
        public Guid Id { get; set; }
        public Guid? AiPromptManagerId { get; set; }
        public string? UserRole { get; set; }
        public string? SettingAiConfig { get; set; }
        public object? JsonConfig { get; set; }
    }
}
