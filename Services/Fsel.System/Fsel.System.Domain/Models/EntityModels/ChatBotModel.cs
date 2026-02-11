// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.Chatbots;
    using global::System;

    public class ChatBotModel : BaseModel
    {
        public Guid? UnitId { get; set; }

        public EnumCourseSkill? Skill { get; set; }
        public EnumChatbotLayout ChatbotLayout { get; set; }
        public long RemainToken { get; set; }

        public Guid StudentId { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
        public EnumChatBotStatus Status { get; set; }

        public IList<ChatBotMessage>? Conversations { get; set; }

        public ChatBotMessage? LastestAnswer { get; set; }

        public ChatBotMessage? LastestQuestion { get; set; }

        public double ProgressRatio { get; set; }
    }
}
