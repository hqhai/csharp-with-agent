// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ClassForumDetailResult : Entity, ISubmissionCount
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Content { get; set; }

        private string? _wordContent;

        public string? WordContent
        {
            get { return _wordContent; }
            set { _wordContent = value; WordCount = StringHelper.CountWords(value); }
        }

        private int? _wordCount;
        private EnumMediaType? _mediaType;

        public int? WordCount
        {
            get { return _wordCount == null ? StringHelper.CountWords(WordContent) : _wordCount; }
            set { _wordCount = value; }
        }

        [NotMapped]
        public EnumMediaType? MediaType
        {
            get { return _mediaType.HasValue ? _mediaType : MediaHelper.GetMediaType(ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault()); }
            set { _mediaType = value; }
        }

        public EnumSubmissionCount? SubmissionCount { get; set; }

        [NotMapped]
        public int? TimeCount
        { get { return ClassForumResultFiles.Select(p => p.TimeCount).Sum(); } }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GradingAlFeedback { get; set; }

        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public EnumClassForumResultStatus Status { get; set; }
        public Guid ClassForumResultId { get; set; }
        public ClassForumResult? ClassForumResult { get; set; }

        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();
    }
}
