// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Enums.ErrorCodes;
    using Common.Helpers;
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
        public Guid? FeatureObjectId { get; set; }
        public Guid? ProjectId { get; set; }
        public AiPromptManager? AiPromptParent { get; set; }
        public ICollection<AICriteriaConfigs> AICriteriaConfigs { get; set; } = new List<AICriteriaConfigs>();

        [NotMapped]
        public object? InputModelJson
        {
            get { return ConvertHelper.Deserialize<object>(InputModel); }
            set { InputModel = ConvertHelper.Serialize(value); }
        }
    }
}
