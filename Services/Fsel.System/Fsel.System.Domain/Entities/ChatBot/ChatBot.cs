// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.ChatBot
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class ChatBot : Entity
    {
        public Guid? UnitId { get; set; }

        public EnumCourseSkill Skill { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }

        public long RemainToken { get; set; }

        public Guid StudentId { get; set; }

        public string? ContentStr { get; set; }

        public string? LastestAnswerStr { get; set; }

        public string? LastestQuestionStr { get; set; }

        public EnumChatBotStatus Status { get; set; }

        [NotMapped]
        public IList<ChatBotMessage>? Conversations
        {
            get
            {
                return ConvertHelper.Deserialize<IList<ChatBotMessage>>(ContentStr);
            }
            set { ContentStr = ConvertHelper.Serialize(value); }
        }

        [NotMapped]
        public ChatBotMessage? LastestAnswer
        {
            get
            {
                return ConvertHelper.Deserialize<ChatBotMessage>(LastestAnswerStr);
            }
            set { LastestAnswerStr = ConvertHelper.Serialize(value); }
        }

        [NotMapped]
        public ChatBotMessage? LastestQuestion
        {
            get
            {
                return ConvertHelper.Deserialize<ChatBotMessage>(LastestQuestionStr);
            }
            set { LastestQuestionStr = ConvertHelper.Serialize(value); }
        }
    }

    public class ChatBotMessage
    {
        public string? Role { get; set; }

        public string? FilePath { get; set; }

        public string? Content { get; set; }
    }
}
