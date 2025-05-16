using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class ChatbotConfigModel : BaseModel
    {
        public string? ProgramName { get; set; }
        public string? CourseName { get; set; }
        public string? CEFRLevel { get; set; }
        public string? UnitNumber { get; set; }

        public string? UnitTopic { get; set; }
        public int NumberSkill { get; set; }
        public EnumChatbotConfigStatus Status { get; set; }
        public Guid UnitId { get; set; }

        public IList<ChatbotSkillConfigModel>? ChatbotSkillConfigs { get; set; }

        public ChatbotTokenConfigsModel? ChatbotTokenConfigs { get; set; }
    }
}
