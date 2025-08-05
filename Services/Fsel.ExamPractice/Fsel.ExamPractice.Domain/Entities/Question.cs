// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.Shared.Enums;

    public class Question : Entity
    {
        /// <summary>
        /// Loại câu hỏi
        /// </summary>
        public EnumQuestionType QuestionType { get; set; }

        /// <summary>
        /// Check câu hỏi có tính điểm không
        /// </summary>
        public bool Ungraded { get; set; }

        /// <summary>
        /// Lưu câu giải thích
        /// </summary>
        public string? Explanation { get; set; }

        [NotMapped]
        public IList<ExplanationTranslationModel>? Explanations
        {
            get { return ConvertHelper.Deserialize<IList<ExplanationTranslationModel>>(Explanation); }
            set
            {
                if (value != null)
                {
                    Explanation = ConvertHelper.Serialize(value);
                }
            }
        }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
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

        public string? SubQuestionIndexsStr { get; set; }

        [NotMapped]
        public int SubQuestionNumber
        {
            get { return SubQuestionIndexs?.Count ?? default; }
        }

        [NotMapped]
        public IList<int>? SubQuestionIndexs
        {
            get { return ConvertHelper.Deserialize<IList<int>>(SubQuestionIndexsStr); }
            set { SubQuestionIndexsStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(3000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public ExamPracticeSection? ExamPracticeSection { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
        public ICollection<ExamPracticeAnswer> ExamPracticeAnswers { get; set; } = new List<ExamPracticeAnswer>();
    }
}
