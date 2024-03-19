// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigModel : BaseModel
    {
        public IList<SkillConfigModel>? Configs { get; set; }
        public string? AiConfig { get; set; }
    }

    #region ChildClass
    public class SkillConfigModel
    {
        public EnumCourseSkill? SkillConfigType { get; set; }
        public IList<SkillBlockItemModel>? BlockItems { get; set; }
    }

    public class SkillBlockItemModel
    {
        public string? Name { get; set; }
        public IList<ItemSkillContentModel>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContentModel
    {
        public EnumGrammarPromptType? ContentType { get; set; }

        public string? Content { get; set; }

    }
    #endregion
}
