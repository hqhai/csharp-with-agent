// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices.CommandModels
{
    using Fsel.Shared.Enums;

    public class SaveChatbotSkillConfigsCommandModel
    {
        public Guid? Id { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public EnumChatbotLayout ChatbotLayout { get; set; }
        public Guid SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
        public int Token { get; set; }
        public string? AiConfig { get; set; }
        public Guid AICriteriaConfigId { get; set; }
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
