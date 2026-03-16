// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Chatbots
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class ChatbotSkillConfig : Entity
    {
        public EnumCourseSkill Skill { get; set; }

        public Guid? SkillId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SkillName { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SkillFilePath { get; set; }

        public EnumChatbotLayout ChatbotLayout { get; set; }

        public int Token { get; set; }

        /// <summary>
        /// Config Real
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Config { get; set; }

        [NotMapped]
        public IList<SkillConfig>? Configs
        {
            get
            {
                return ConvertHelper.Deserialize<IList<SkillConfig>>(Config);
            }
            set { Config = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Ai Config
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? AiConfig { get; set; }

        public Guid AICriteriaConfigId { get; set; }
        public Guid ChatbotConfigId { get; set; }
        public ChatbotConfig? ChatbotConfig { get; set; }
    }

    public class SkillConfig
    {
        public string? Name { get; set; }
        public IList<ItemSkillContent>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContent
    {
        public EnumGrammarPromptType? ContentType { get; set; }
        public string? Content { get; set; }
    }
}
