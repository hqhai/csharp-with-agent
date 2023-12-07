// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IEntities;

    public class VideoTimeCodeResult : BaseResultScore, IHighestStreak, IWorkingTime
    {
        public VideoResult? VideoResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoResultId { get; set; }

        public VideoTimeCode? VideoTimeCode { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoTimeCodeId { get; set; }

        /// <summary>
        /// Thời gian còn lại
        /// </summary>
        public double WorkingTime { get; set; }

        /// <summary>
        /// Thời gian làm lại bài còn lại
        /// </summary>
        public double RetryWorkingTime { get; set; }

        /// <summary>
        /// Đang làm việc
        /// </summary>
        public bool IsWorking { get; set; }

        public int? HighestStreak { get; set; }
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
