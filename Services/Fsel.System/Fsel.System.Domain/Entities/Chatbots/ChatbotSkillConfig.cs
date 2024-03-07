// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Chatbots
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class ChatbotSkillConfig : Entity
    {
        public EnumCourseSkill Skill { get; set; }

        /// <summary>
        /// Config Real
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Config { get; set; }

        /// <summary>
        /// Ai Config
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? AiConfig { get; set; }

        public Guid ChatbotConfigId { get; set; }
        public ChatbotConfig? ChatbotConfig { get; set; }
    }
}
