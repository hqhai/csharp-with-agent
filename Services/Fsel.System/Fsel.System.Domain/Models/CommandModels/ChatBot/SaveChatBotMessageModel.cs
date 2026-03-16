// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatBot
{
    public class SaveChatBotMessageModel
    {
        public Guid UnitId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SkillId { get; set; }
        public Guid UnitResultId { get; set; }
    }
}
