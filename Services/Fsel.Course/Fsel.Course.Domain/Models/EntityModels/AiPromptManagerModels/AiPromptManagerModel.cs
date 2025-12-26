// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class AiPromptManagerModel : BaseModel
    {
        public string? AiModelName { get; set; }
        public object? InputModelJson { get; set; }
        public Guid? FeatureObjectId { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
