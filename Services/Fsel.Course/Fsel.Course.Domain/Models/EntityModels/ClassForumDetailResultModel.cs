// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
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

        public int? TimeCount { get; set; }
        public string? GradingAlFeedback { get; set; }

        public string? PronunciationAlFeedback { get; set; }

        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public double Score { get; set; }
        public double CorrectCount { get; set; }
        public double CorrectTotal { get; set; }
        public double PronunciationScore { get; set; }
        public EnumSubmissionCount? SubmissionCount { get; set; }
        public EnumClassForumResultStatus Status { get; set; }

        public Guid ClassForumResultId { get; set; }

        [JsonIgnore]
        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }

        public IList<string>? FilePaths
        { get { return ClassForumResultFiles?.Select(x => x.FilePath ?? string.Empty).ToList(); } }

        public IList<ClassForumDetailResultHistoryModel>? ClassForumDetailResultHistories { get; set; }
    }
}
