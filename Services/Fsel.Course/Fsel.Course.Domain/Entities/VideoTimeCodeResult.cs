// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Helpers;

    public class VideoTimeCodeResult : BaseLearnResult, ITokenResult
    {
        [NotMapped]
        public override double PercentModule { get; set; }

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
        /// Tổng điểm đạt được của câu hỏi không tính điểm
        /// </summary>
        public int? CorrectCountUngraded { get; set; }

        /// <summary>
        /// Tổng điểm của câu hỏi không tính điểm đúng
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
                return CorrectTotal + (CorrectTotalUngraded ?? default) > 0 ? NumberHelper.GetPercent(CorrectCount + (CorrectCountUngraded ?? default), CorrectTotal + (CorrectTotalUngraded ?? default)) : _percentUngraded;
            }
            set { _percentUngraded = CorrectTotal + (CorrectTotalUngraded ?? default) > 0 ? NumberHelper.GetPercent(CorrectCount + (CorrectCountUngraded ?? default), CorrectTotal + (CorrectTotalUngraded ?? default)) : value; }
        }

        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }

        /// <summary>
        /// Chuỗi liên tiếp của sub question
        /// </summary>
        public int? HighestStreakSubQuestion { get; set; }

        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
