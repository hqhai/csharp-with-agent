// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using global::System;

    public class ChatbotConfigModel : ChatbotSkillConfigModel
    {
        public Guid Id { get; set; }
        public string? ProgramName { get; set; }
        public string? CourseName { get; set; }
        public string? CEFRLevel { get; set; }
        public string? UnitNumber { get; set; }
        public int NumberSkill { get; set; }
        public EnumChatbotConfigStatus Status { get; set; }
        public Guid UnitId { get; set; }
    }
}
