// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Shared.Helpers;
    using Fsel.Course.Domain.IEntities;

    public class VideoTimeCodeResult : BaseLearnResult, ITokenResult
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
        public int? CorrectCountUngraded { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        public int? CorrectTotalUngraded { get; set; }

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
        private double _percentUngraded;

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public override double Percent
        {
            get
            {
                return CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount + (CorrectCountUngraded ?? default), CorrectTotal + (CorrectTotalUngraded ?? default)) : _percentUngraded;
            }
            set { _percentUngraded = CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount + (CorrectCountUngraded ?? default), CorrectTotal + (CorrectTotalUngraded ?? default)) : value; }
        }
        public int? TokenDone { get; set; }
        public int? TokenHighestStreak { get; set; }
        public int? TokenSuperFire { get; set; }
        public int? TokenQuestionReward { get; set; }
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}