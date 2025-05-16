// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Helpers;

    public class ExamPracticeResult : Entity
    {
        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal { get; set; }

        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public virtual double Percent { get; set; }

        [NotMapped]
        private double PercentValue
        {
            get
            {
                return CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : PercentValue;
            }
            set
            {
                Percent = CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : value;
            }
        }

        public string? SkillScoresStr { get; set; }

        [NotMapped]
        public IList<SkillScores>? SkillScores
        {
            get
            {
                return ConvertHelper.Deserialize<IList<SkillScores>>(SkillScoresStr);
            }
            set { SkillScoresStr = ConvertHelper.Serialize(value); }
        }

        public EnumPracticeMode? PracticeMode { get; set; }
        public string? ExerciseConfig { get; set; }

        /// <summary>
        /// Chuỗi liên tiếp
        /// </summary>
        public int? HighestStreak { get; set; }

        /// <summary>
        /// Thời gian còn lại
        /// </summary>
        public double WorkingTime { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public Guid StudentId { get; set; }

        public int ResultPosition { get; set; }
        public Guid ExamPracticeRetryId { get; set; }
        public ExamPracticeRetry? ExamPracticeRetry { get; set; }
        public ExamPractice? ExamPractice { get; set; }
        public Guid ExamPracticeId { get; set; }
        public ICollection<ExamPracticeSectionResult> ExamPracticeSectionResults { get; set; } = new List<ExamPracticeSectionResult>();
        public ICollection<ExamPracticeAnswer> ExamPracticeAnswers { get; set; } = new List<ExamPracticeAnswer>();
    }
}
