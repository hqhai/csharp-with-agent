// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Enums.ErrorCodes;
    using Core.Entities;

    [Table("AiModelManager")]
    public class AiModelManager : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AiModelName { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? InputModel { get; set; }
        public ICollection<AiModelFeature> AiModelFeatures { get; set; } = new List<AiModelFeature>();
    }
}
