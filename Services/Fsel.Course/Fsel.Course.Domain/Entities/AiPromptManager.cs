// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Common.Enums.ErrorCodes;
    using Core.Entities;
    using Fsel.Common.Enums;
    using Fsel.Shared.Enums;

    public class AiPromptManager : Entity
    {
        /// <summary>
        /// Trạng thái version (LastVersion, OldVersion)
        /// </summary>
        public EnumVersionStatus VersionStatus { get; set; }

        /// <summary>
        /// Số phiên bản
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// ID của entity gốc (null cho version đầu tiên)
        /// </summary>
        public Guid? OriginalId { get; set; }

        /// <summary>
        /// Loại version (V1, V2, V3)
        /// </summary>
        public EnumVersion VersionType { get; set; } = EnumVersion.V2;

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AiModel { get; set; }

        public Guid? FeatureObjectId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AiPromptManagerParentId { get; set; }
        public AiPromptManager? AiPromptParent { get; set; }
        public ICollection<AiPromptManager> AiPromptChildren { get; set; } = new List<AiPromptManager>();
        public ICollection<AICriteriaConfigs> AICriteriaConfigs { get; set; } = new List<AICriteriaConfigs>();
    }
}
