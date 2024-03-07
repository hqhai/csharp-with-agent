// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatbotConfigs
{
    using Fsel.System.Domain.Models.EntityModels;

    public class SaveChatbotConfigCommandModel : ChatbotSkillConfigModel
    {
        public string? ProgramName { get; set; }
        public string? CourseName { get; set; }
        public string? CEFRLevel { get; set; }
        public string? UnitNumber { get; set; }
        public int NumberSkill { get; set; }
        public Guid UnitId { get; set; }
    }
}
