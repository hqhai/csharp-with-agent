// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigModel : BaseModel
    {
        public IList<SkillConfigModel>? Configs { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
        public int? Token { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public string? AiConfig { get; set; }
    }

    #region ChildClass

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

    #endregion ChildClass
}
