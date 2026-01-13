// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Enums;

    public class CreateAiCriteriaConfigCommandModel

    {
        public Guid? Id { get; set; }
        public Guid AiPromptManagerId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ObjectId { get; set; }
        public EnumCriteriaAi? TypeCriteriaAi { get; set; }
        public EnumSubFeatureType? SubFeatureType { get; set; }
        public EnumFeatureMultiple? FeatureMultiple { get; set; }
        public EnumDefaultType DefaultType { get; set; }
        public string? UserRole { get; set; }
        public string? SettingAiConfig { get; set; }
        public object? JsonConfig { get; set; }
    }
}
