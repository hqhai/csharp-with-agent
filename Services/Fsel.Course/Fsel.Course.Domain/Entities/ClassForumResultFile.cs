// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Helpers;

    public class ClassForumResultFile : Entity
    {
        /// <summary>
        /// File Link
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        [NotMapped]
        public int? TimeCount
        { get { return MediaHelper.GetMediaDurationAsync(FilePath); } }

        public bool IsRetry { get; set; }

        public Guid ClassForumResultId { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
