// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    public class UpdateAiCriteriaCommandModel
    {
        public Guid Id { get; set; }
        public Guid? AiPromptManagerId { get; set; }
        public string? UserRole { get; set; }
        public string? SettingAiConfig { get; set; }
        public object? JsonConfig { get; set; }
    }
}
