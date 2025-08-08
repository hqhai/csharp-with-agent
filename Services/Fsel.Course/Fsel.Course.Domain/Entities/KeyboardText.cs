// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;

    public class KeyboardText : Entity
    {
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public int Unicode { get; set; }

        [MaxLength(5000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        public Guid KeyboardLayoutId { get; set; }

        public KeyboardLayout? KeyboardLayout { get; set; }
    }
}
