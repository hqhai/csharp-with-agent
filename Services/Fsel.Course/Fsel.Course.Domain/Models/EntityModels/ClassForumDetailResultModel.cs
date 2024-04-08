// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ClassForumDetailResultModel : BaseModel
    {
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

        public string? GradingAlFeedback { get; set; }

        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public EnumSubmissionCount? SubmissionCount { get; set; }

        public Guid ClassForumResultId { get; set; }

        public ClassForumResultFileModel? ClassForumResultFile { get; set; }
    }
}
