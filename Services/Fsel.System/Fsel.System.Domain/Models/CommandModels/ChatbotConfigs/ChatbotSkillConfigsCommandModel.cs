// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatbotConfigs
{
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigsCommandModel
    {
        public IList<SkillConfigCommandModel>? Configs { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public string? AiConfig { get; set; }

        public Guid? Id { get; set; }
    }

    #region ChildClass
    public class SkillConfigCommandModel
    {
        public string? Name { get; set; }

        public IList<ItemSkillContentCommandModel>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContentCommandModel
    {
        public EnumGrammarPromptType? ContentType { get; set; }

        public string? Content { get; set; }

    }
    #endregion
}
