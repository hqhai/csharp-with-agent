// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatBot
{
    using Fsel.Shared.Enums;

    public class SaveChatBotMessageModel
    {
        public Guid? UnitId { get; set; }
        public EnumCourseSkill Skill { get; set; }
        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public Guid? StudentId { get; set; }
    }
}
