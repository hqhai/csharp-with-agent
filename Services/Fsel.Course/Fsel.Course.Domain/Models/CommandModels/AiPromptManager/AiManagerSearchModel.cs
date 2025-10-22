// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiPromptManager
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class AiManagerSearchModel : BaseModel
    {
        public string? AiModelName { get; set; }
        public string? InputModel { get; set; }
        public Guid? ParentId { get; set; }
        public EnumFeature FeatureAi { get; set; }
        public Guid? FeatureObjectId { get; set; }
    }
}
