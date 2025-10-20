// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelManager
{
    public class UpdateAiPromptCommandManagerModel
    {
        public Guid Id { get; set; }
        public string? AiModelName { get; set; }
        public string? InputModel { get; set; }
    }
}
