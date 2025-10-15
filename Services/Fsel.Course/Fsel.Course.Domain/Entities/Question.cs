// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Entities.TestConfigs;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;

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
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
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

        public ICollection<ExerciseQuestion> ExerciseQuestions { get; set; } = new List<ExerciseQuestion>();
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<HomeWorkQuestion> HomeWorkQuestions { get; set; } = new List<HomeWorkQuestion>();
        public ICollection<SectionQuestion> SectionQuestions { get; set; } = new List<SectionQuestion>();

        public ICollection<QuestionExplanationError> QuestionExplanationErrors = new List<QuestionExplanationError>();
        public ICollection<QuestionExplanationLog> QuestionExplanationLogs { get; set; } = new List<QuestionExplanationLog>();
        public ICollection<QuestionShuffle> QuestionShuffles { get; set; } = new List<QuestionShuffle>();
        public ICollection<TestSectionQuestion> TestSectionQuestions { get; set; } = new List<TestSectionQuestion>();
        public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
    }
}
