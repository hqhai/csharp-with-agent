// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelFeature
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;

    public class CreateAiFeatureCommandModel
    {
        public Guid FeatureObjectId { get; set; }
        public Guid AiModelManagerId { get; set; }
        public EnumFeatureAi FeatureAi { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public object? JsonConfig { get; set; }
        public SetiingAiFeatureModel? Setting { get; set; }
        public List<CreateAiFeatuerHasSubCommadModel>? SubFeature { get; set; }
    }
}
