// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class ExtraPracticeAnswer : Entity
    {
        /// <summary>
        /// Câu trả lời
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answer
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        public Question? Question { get; set; }
        public ExtraPracticeResult? ExtraPracticeResult { get; set; }
        public ExtraPracticeExerciseResult? ExtraPracticeExerciseResult { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Section? Section { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public Guid? ExtraPracticeExerciseResultId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? SectionId { get; set; }
    }
}
