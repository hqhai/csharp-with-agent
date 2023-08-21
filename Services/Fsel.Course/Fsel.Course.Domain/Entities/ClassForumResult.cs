// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ClassForumResult : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Content { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? WordContent { get; set; }

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

        [Range(0, 5, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? FeedBackStars { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBackNote { get; set; }

        public bool? IsFlagged { get; set; }

        public ClassForum? ClassForum { get; set; }

        public LessonResult? LessonResult { get; set; }
        public string? FeedBackPositivesStr { get; set; }

        public Guid? CheckCsoId { get; set; }

        public DateTime? CheckStartDate { get; set; }

        public DateTime? GradingStartDate { get; set; }

        [NotMapped]
        public IList<EnumFeedBackPositive>? FeedBackPositives
        {
            get { return ConvertHelper.Deserialize<IList<EnumFeedBackPositive>>(FeedBackPositivesStr); }
            set { FeedBackPositivesStr = ConvertHelper.Serialize(value); }
        }

        public string? FeedBackNegativesStr { get; set; }

        [NotMapped]
        public IList<EnumFeedBackNegative>? FeedBackNegatives
        {
            get { return ConvertHelper.Deserialize<IList<EnumFeedBackNegative>>(FeedBackNegativesStr); }
            set { FeedBackNegativesStr = ConvertHelper.Serialize(value); }
        }
        public ICollection<ClassForumScore> ClassForumScores { get; set; } = new List<ClassForumScore>();

        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();
    }
}
