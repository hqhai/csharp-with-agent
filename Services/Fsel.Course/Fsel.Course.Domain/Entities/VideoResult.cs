// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class VideoResult : Entity
    {
        /// <summary>
        /// Phần trăm câu trả lời
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double Percent { get; set; }

        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public LessonResult? LessonResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        public Video? Video { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoId { get; set; }

        public Guid StudentId { get; set; }
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
