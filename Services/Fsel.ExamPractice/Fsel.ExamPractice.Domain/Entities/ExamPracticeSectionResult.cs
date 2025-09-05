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

    public class ExamPracticeSectionResult : Entity
    {
        private int _correctCount;

        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount
        {
            get => _correctCount;
            set
            {
                _correctCount = value;
                UpdatePercent();
            }
        }

        private int _correctTotal;

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal
        {
            get => _correctTotal;
            set
            {
                _correctTotal = value;
                UpdatePercent();
            }
        }

        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public virtual double Percent { get; set; }

        private void UpdatePercent()
        {
            Percent = CorrectTotal > 0
                ? NumberHelper.GetPercent(CorrectCount, CorrectTotal)
                : Percent;
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

        public Guid? CurrentExamPracticeSectionId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public ExamPracticeResult? ExamPracticeResult { get; set; }
        public Guid ExamPracticeResultId { get; set; }

        public ExamPracticeSectionResult? ParentExamPracticeSectionResult { get; set; }
        public Guid? ParentExamPracticeSectionResultId { get; set; }

        public ExamPracticeSection? ExamPracticeSection { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
        public ICollection<ExamPracticeSectionResult> ExamPracticeSectionResults { get; set; } = new List<ExamPracticeSectionResult>();
        public ICollection<ExamPracticeAnswer> ExamPracticeAnswers { get; set; } = new List<ExamPracticeAnswer>();
    }
}
