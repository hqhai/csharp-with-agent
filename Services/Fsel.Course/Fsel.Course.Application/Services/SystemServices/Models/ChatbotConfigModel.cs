// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ChatbotConfigModel : BaseModel
    {
        public string? ProgramName { get; set; }
        public Guid ProgramId { get; set; }
        public string? CourseName { get; set; }
        public string? CEFRLevel { get; set; }
        public string? UnitNumber { get; set; }
        public string? UnitTopic { get; set; }
        public int NumberSkill { get; set; }
        public EnumChatbotConfigStatus Status { get; set; }
        public Guid UnitId { get; set; }
    }
}
