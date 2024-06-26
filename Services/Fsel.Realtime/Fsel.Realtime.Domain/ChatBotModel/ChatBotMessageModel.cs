using Fsel.Shared.Enums;

namespace Fsel.Realtime.Domain.ChatBotModel
{
    public class ChatBotMessageModel
    {
        public string? Role { get; set; }
        public string? Content { get; set; }

        public Guid? ChatBotId { get; set; }

        public EnumCourseSkill Skill { get; set; }
    }
}
