// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelManager
{
    public class UpdateAiModelCommandManagerModel
    {
        public Guid Id { get; set; }
        public string? AiModelName { get; set; }
        public string? InputModel { get; set; }
    }
}
