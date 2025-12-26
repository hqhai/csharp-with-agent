// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiPromptManager
{
    public class CreateAiPromptManagerCommandModel
    {
        public string? Name { get; set; }
        public object? AiModel { get; set; }
        public Guid? FeatureObjectId { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
