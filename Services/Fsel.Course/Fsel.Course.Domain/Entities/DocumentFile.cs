// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Course.Domain.Enums;

    public class DocumentFile
    {
        public EnumDocumentType Type { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }
    }
}
