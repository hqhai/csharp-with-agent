// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class RubyScope : Entity
    {
        /// <summary>
        /// Enity chứa nội dung
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? HostType { get; set; }
        /// <summary>
        /// Id của nội dung cần gán 
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HostId { get; set; }
        /// <summary>
        /// Loại nội dung 
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? FieldKey { get; set; }
        /// <summary>
        /// Nội dung cần gán
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Text { get; set; }
        /// <summary>
        /// Độ dài đoạn text
        /// </summary>
        public int? BaseLengthGraphemes { get; set; }
        /// <summary>
        /// Tránh đồng bị khi nhiều người chỉnh sửa
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;
        public ICollection<RubyAnnotation> RubyAnnotations { get; set; } = new List<RubyAnnotation>();
    }
}
