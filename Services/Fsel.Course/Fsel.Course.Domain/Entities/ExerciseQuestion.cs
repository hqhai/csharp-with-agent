// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class ExerciseQuestion : Entity
    {
        public Exercise? Exercise { get; set; }

        public Question? Question { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ExerciseId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestionId { get; set; }
        //public ICollection<ExtraPracticeExerciseAnswer> ExtraPracticeExerciseAnswers { get; set; } = new List<ExtraPracticeExerciseAnswer>();
    }
}
