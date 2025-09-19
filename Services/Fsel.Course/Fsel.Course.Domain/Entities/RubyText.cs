// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class RubyText : Entity
    {
        /// <summary>
        /// Loại ngôn ngữ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public EnumLanguageType LanguageType { get; set; }

        /// <summary>
        /// Văn bản gốc cần note ruby text
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? BaseText { get; set; }

        /// <summary>
        /// Ruby text của văn bản gốc
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Phonetic { get; set; }
    }
}
