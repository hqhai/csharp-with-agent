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

    public class ExtraPracticeResult : Entity
    {
        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>
        private double _percent;

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double Percent
        {
            get
            {
                return CorrectTotal > 0 ? ((double)CorrectCount / CorrectTotal * 100) : _percent;
            }
            set { _percent = CorrectTotal > 0 ? ((double)CorrectCount / CorrectTotal * 100) : value; }
        }

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
        /// Trạng thái
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public string? SkillScoresStr { get; set; }

        [NotMapped]
        public IList<SkillScores>? SkillScores
        {
            get { return ConvertHelper.Deserialize<IList<SkillScores>>(SkillScoresStr); }
            set { SkillScoresStr = ConvertHelper.Serialize(value); }
        }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public Guid ExtraPracticeId { get; set; }

        public Guid? CurrentVideoTimeCodeId { get; set; }
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; } = new List<ExtraPracticeExerciseResult>();
    }
}
