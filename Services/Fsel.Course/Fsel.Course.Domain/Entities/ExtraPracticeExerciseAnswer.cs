// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class ExtraPracticeExerciseAnswer : Entity
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

        public ExtraPracticeExercise? ExtraPracticeExercise { get; set; }
        public ExerciseQuestion? ExerciseQuestion { get; set; }

        public Guid ExtraPracticeExerciseId { get; set; }
        public Guid ExerciseQuestionId { get; set; }
        public Guid StudentId { get; set; }
    }
}
