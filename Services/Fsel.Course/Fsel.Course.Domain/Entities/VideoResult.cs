// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Newtonsoft.Json;

    public class VideoResult : Entity
    {
        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
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
        /// Số sao
        /// </summary>
        [Range(0, 5, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double NumberOfStars { get; set; }

        /// <summary>
        /// Số sao
        /// </summary>
        [MaxLength(10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Feedback { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public string? VideoSkillScoresStr { get; set; }

        [NotMapped]
        public IList<VideoSkillScores>? VideoSkillScores
        {
            get
            {
                return ConvertHelper.Deserialize<IList<VideoSkillScores>>(VideoSkillScoresStr);
            }
            set { VideoSkillScoresStr = ConvertHelper.Serialize(value); }
        }

        public LessonResult? LessonResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        public Guid? CurrentVideoTimeCodeId { get; set; }
        public Video? Video { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoId { get; set; }

        public Guid StudentId { get; set; }
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
