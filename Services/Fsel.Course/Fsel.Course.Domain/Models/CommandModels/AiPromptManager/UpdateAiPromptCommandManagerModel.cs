// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiPromptManager
{
    public class UpdateAiPromptCommandManagerModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? JsonAiModel { get; set; }
        public Guid? FeatureObjectId { get; set; }
    }
}
