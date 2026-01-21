// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Chatbots
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class ChatBot : Entity
    {
        public Guid? UnitId { get; set; }

        public EnumCourseSkill Skill { get; set; }
        public Guid? SkillId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SkillName { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SkillFilePath { get; set; }

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
                return ContentStr.Deserialize<IList<ChatBotMessage>>();
            }
            set { ContentStr = value.Serialize(); }
        }

        [NotMapped]
        public ChatBotMessage? LastestAnswer
        {
            get
            {
                return LastestAnswerStr.Deserialize<ChatBotMessage>();
            }
            set { LastestAnswerStr = value.Serialize(); }
        }

        [NotMapped]
        public ChatBotMessage? LastestQuestion
        {
            get
            {
                return LastestQuestionStr.Deserialize<ChatBotMessage>();
            }
            set { LastestQuestionStr = value.Serialize(); }
        }
    }

    public class ChatBotMessage
    {
        public string? Role { get; set; }

        public string? FilePath { get; set; }

        public string? Content { get; set; }
    }
}
