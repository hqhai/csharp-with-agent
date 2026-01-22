// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Chatbots
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class ChatbotConfig : Entity
    {
        /// <summary>
        /// Program Name
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ProgramName { get; set; }

        public Guid ProgramId { get; set; }

        /// <summary>
        /// Course Name
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CourseName { get; set; }

        /// <summary>
        /// CEFR Level
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CEFRLevel { get; set; }

        /// <summary>
        /// Unit Number
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UnitNumber { get; set; }

        /// <summary>
        /// Unit Number
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UnitTopic { get; set; }

        public int NumberSkill { get; set; }
        public EnumChatbotConfigStatus Status { get; set; }
        public Guid UnitId { get; set; }

        public ChatbotTokenConfigs? ChatbotTokenConfigs { get; set; }
        public ICollection<ChatbotSkillConfig> ChatbotSkillConfigs { get; set; } = new List<ChatbotSkillConfig>();
    }
}
