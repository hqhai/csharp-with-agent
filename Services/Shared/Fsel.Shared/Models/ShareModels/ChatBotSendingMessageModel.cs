// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class ChatBotSendingMessageModel
    {
        public string? Message { get; set; }

        public Guid? ChatbotId { get; set; }

        public string? FilePath { get; set; }
    }

    public class ChatBotRealTimeModel : ChatBotSendingMessageModel
    {
        public EnumCourseSkill Skill { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? StudentId { get; set; }
    }
}
