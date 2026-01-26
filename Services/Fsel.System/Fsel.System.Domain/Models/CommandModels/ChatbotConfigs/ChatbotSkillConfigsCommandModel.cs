// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatbotConfigs
{
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigsCommandModel
    {
        public Guid? Id { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public EnumChatbotLayout ChatbotLayout { get; set; }
        public Guid SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
        public int Token { get; set; }
        public string? AiConfig { get; set; }
        public IList<SkillConfigCommandModel>? Configs { get; set; }
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

    #endregion ChildClass
}
