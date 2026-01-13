// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels
{
    using Core.Base.BaseModels;

    public class AiPromptManagerModel : BaseModel
    {
        public string? Name { get; set; }
        public string? AiModel { get; set; }
        public Guid? FeatureObjectId { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
