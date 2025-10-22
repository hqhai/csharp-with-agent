// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Common.Enums.ErrorCodes;
    using Core.Entities;
    using Fsel.Shared.Enums;

    public class AiPromptManager : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AiModelName { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? InputModel { get; set; }
        public EnumFeature FeatureAi { get; set; }
        public Guid? FeatureObjectId { get; set; }
        public Guid? ParentId { get; set; }
        public AiPromptManager? AiPromptParent { get; set; }
        public ICollection<AiPromptManager> AiPromptManagers { get; set; } = new List<AiPromptManager>();
        public ICollection<AICriteriaConfigs> AICriteriaConfigs { get; set; } = new List<AICriteriaConfigs>();
    }
}
