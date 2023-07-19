// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class MockTestAnswer : Entity
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
        /// Số lượng câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        public Guid? SectionQuestionId { get; set; }

        public SectionQuestion? SectionQuestion { get; set; }

        public Guid MockTestResultId { get; set; }
        public MockTestResult? MockTestResult { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Guid? SectionId { get; set; }
        public Section? Section { get; set; }

    }
}
