// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices.CommandModels
{
    using System;
    using System.Collections.Generic;

    public class SaveChatbotConfigCommandModel
    {
        public string? ProgramName { get; set; }
        public Guid ProgramId { get; set; }
        public string? CourseName { get; set; }
        public string? CEFRLevel { get; set; }
        public string? UnitNumber { get; set; }
        public int NumberSkill { get; set; }
        public Guid UnitId { get; set; }
        public string? UnitTopic { get; set; }
        public IList<SaveChatbotSkillConfigsCommandModel> ChatbotSkillConfigs { get; set; } = new List<SaveChatbotSkillConfigsCommandModel>();
    }
}
