// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class RubyAnnotation : Entity
    {
        /// <summary>
        /// FK với RubyScope
        /// </summary>
        public Guid RubyScopeId { get; set; }
        /// <summary>
        /// Loại ngôn ngữ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public EnumLanguageType LanguageType { get; set; }

        /// <summary>
        /// Văn bản gốc cần note ruby text
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? SelectedText { get; set; }

        /// <summary>
        /// Ruby text của văn bản gốc
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Phonetic { get; set; }

        /// <summary>
        /// Vị trí bắt đầu nội dung cần gắn rubytext trong Document.Content
        /// </summary>
        [Range(0, 100000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int StartGraphemeIndex { get; set; }

        /// <summary>
        /// Độ dài
        /// </summary>
        [Range(1, 5, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int LengthGraphemes { get; set; }

        /// <summary>
        /// Tránh đồng bộ
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; } = default!;
        public RubyScope Scope { get; set; } = default!;
    }
}
