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
    using Fsel.Shared.Helpers;

    public class ClassForumResult : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Content { get; set; }

        public string? WordContent { get; set; }

        [NotMapped]
        public int WordCount
        { get { return StringHelper.CountWords(WordContent); } }

        [NotMapped]
        public int? TimeCount
        { get { return ClassForumResultFiles.Select(p => p.TimeCount).Sum(); } }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GradingAlFeedback { get; set; }

        public Guid? GradingTeacherId { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumClassForumResultStatus Status { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ClassForumId { get; set; }

        public ClassForum? ClassForum { get; set; }

        public LessonResult? LessonResult { get; set; }

        public Guid? CheckCsoId { get; set; }

        public DateTime? CheckStartDate { get; set; }

        public DateTime? GradingStartDate { get; set; }

        public string? RetryContent { get; set; }

        public string? RetryWordContent { get; set; }

        public string? RetryGradingAlFeedBack { get; set; }

        public ICollection<ClassForumScore> ClassForumScores { get; set; } = new List<ClassForumScore>();

        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();

        public ICollection<ClassForumResultRandom> ClassForumResultRandoms { get; set; } = new List<ClassForumResultRandom>();
    }
}
