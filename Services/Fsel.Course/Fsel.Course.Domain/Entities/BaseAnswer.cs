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
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answer
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public EnumAnswerStatus Status { get; set; }

        private bool? _isCorrect;
        private int _correctCount;

        public bool? IsCorrect
        {
            get
            {
                return Status == EnumAnswerStatus.Done ? _isCorrect : null;
            }
            set
            {
                _isCorrect = value;
            }
        }

        /// <summary>
        /// Số lượng câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount
        {
            get
            {
                return Status == EnumAnswerStatus.Done ? _correctCount : default;
            }
            set
            {
                _correctCount = value;
            }
        }
    }
}
