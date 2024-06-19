// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class BaseAnswer : Entity
    {
        /// <summary>
        /// Câu trả lời
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public virtual string? AnswerStr { get; set; }

        [NotMapped]
        public virtual object? Answer
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public EnumAnswerStatus Status { get; set; }
        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Số lượng câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public virtual int CorrectCount { get; set; }
    }
}
