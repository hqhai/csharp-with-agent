// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ClassForumResultFile : Entity
    {
        /// <summary>
        /// File Link
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        public bool IsRetry { get; set; }

        public Guid ClassForumResultId { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
