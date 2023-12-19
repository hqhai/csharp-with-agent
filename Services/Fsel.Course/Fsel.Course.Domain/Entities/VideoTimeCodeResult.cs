// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class VideoTimeCodeResult : BaseLearnResult
    {
        public VideoResult? VideoResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoResultId { get; set; }

        public VideoTimeCode? VideoTimeCode { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoTimeCodeId { get; set; }

        /// <summary>
        /// Thời gian làm lại bài còn lại
        /// </summary>
        public double RetryWorkingTime { get; set; }

        /// <summary>
        /// Đang làm việc
        /// </summary>
        public bool IsWorking { get; set; }

        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCountUngraded { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotalUngraded { get; set; }

        public string? SkillScoreUngradedStr { get; set; }

        [NotMapped]
        public IList<SkillScores>? SkillScoreUngraded
        {
            get
            {
                return ConvertHelper.Deserialize<IList<SkillScores>>(SkillScoreUngradedStr);
            }
            set { SkillScoreUngradedStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
