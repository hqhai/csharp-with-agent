// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class PlacementTestAnswer : Entity
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
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        public PlacementTestResult? PlacementTestResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid PlacementTestResultId { get; set; }

        public SectionQuestion? SectionQuestion { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid SectionQuestionId { get; set; }
    }
}
