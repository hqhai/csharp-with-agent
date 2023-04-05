// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Question : Entity
    {
        /// <summary>
        /// Loại câu hỏi
        /// </summary>
        public EnumQuestionType QuestionType { get; set; }

        /// <summary>
        /// Check câu hỏi có tính điểm không
        /// </summary>
        private bool _ungraded;

        public bool Ungraded
        {
            get { return _ungraded; }
            set { _ungraded = QuestionType == EnumQuestionType.ExercisePreparation || value; }
        }

        /// <summary>
        /// Lưu câu giải thích
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Explanation { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal { get; set; }

        /// <summary>
        /// Config
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<ExerciseQuestion> ExerciseQuestions { get; set; } = new List<ExerciseQuestion>();
        public ICollection<VideoTimeCodeAnswer>? VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
    }
}
