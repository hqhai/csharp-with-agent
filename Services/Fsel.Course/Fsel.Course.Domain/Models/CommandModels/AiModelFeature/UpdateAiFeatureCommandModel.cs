// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelFeature
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class UpdateAiFeatureCommandModel
    {
        public EnumFeature Key { get; set; }
        public Guid? FeatureObjectId { get; set; }
        public Guid? AiPromptManagerId { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public object? JsonConfig { get; set; }
        public List<UpdateHasSubFeatureModel>? SubFeature { get; set; }
    }


    public class UpdateHasSubFeatureModel
    {
        public EnumTypeFeatureAi TypeFeatureAi { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public object? JsonConfig { get; set; }
    }
}
