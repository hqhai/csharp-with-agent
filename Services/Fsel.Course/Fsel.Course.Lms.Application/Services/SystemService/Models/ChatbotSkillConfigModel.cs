using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class ChatbotSkillConfigModel : BaseModel
    {
        public IList<SkillConfigModel>? Configs { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public string? AiConfig { get; set; }
    }

    public class SkillConfigModel
    {
        public string? Name { get; set; }

        public IList<ItemSkillContentModel>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContentModel
    {
        public EnumGrammarPromptType? ContentType { get; set; }

        public string? Content { get; set; }
    }
}
