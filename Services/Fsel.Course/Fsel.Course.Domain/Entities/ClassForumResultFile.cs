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
        private string? _filePath;

        /// <summary>
        /// File Link
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath
        {
            get { return _filePath; }
            set { _filePath = value; TimeCount = MediaHelper.GetMediaDurationAsync(value); }
        }

        private int? _timeCount;

        public int? TimeCount
        {
            get { return _timeCount == null ? MediaHelper.GetMediaDurationAsync(FilePath) : _timeCount; }
            set { _timeCount = value; }
        }

        public bool IsRetry { get; set; }

        public Guid ClassForumResultId { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
