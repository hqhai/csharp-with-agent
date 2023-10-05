// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class VideoTimeCode : Entity
    {
        /// <summary>
        /// Thời gian bắt đầu xuất hiện TimeCode
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double DisplayTime { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double ExecutionTime { get; set; }

        /// <summary>
        /// Loại TimeCode
        /// </summary>
        public EnumTimeCodeType TimeCodeType { get; set; }

        public Video? Video { get; set; }
        public Guid VideoId { get; set; }

        [NotMapped]
        public TimeSpan DisplayTimeSpan
        {
            get { return TimeSpan.FromSeconds(DisplayTime); }
        }

        [NotMapped]
        public TimeSpan ExecutionTimeSpan
        {
            get { return TimeSpan.FromSeconds(ExecutionTime); }
        }

        public ICollection<TimeCodeExercise> TimeCodeExercises { get; set; } = new List<TimeCodeExercise>();
        public ICollection<VideoTimeCodeResult> VideoTimeCodeResults { get; set; } = new List<VideoTimeCodeResult>();
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
    }
}
