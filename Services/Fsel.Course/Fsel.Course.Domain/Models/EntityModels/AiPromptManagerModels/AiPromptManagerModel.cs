// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class AiPromptManagerModel : BaseModel
    {
        public string? AiModelName { get; set; }
        public string? InputModel { get; set; }
        public Guid? ParentId { get; set; }
        public EnumFeature FeatureAi { get; set; }
        public Guid? FeatureObjectId { get; set; }
    }
}
