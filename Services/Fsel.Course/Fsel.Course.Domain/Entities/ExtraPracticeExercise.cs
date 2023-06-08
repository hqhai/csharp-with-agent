// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ExtraPracticeExercise : Entity
    {
        public ExtraPracticeChapter? ExtraPracticeChapter { get; set; }
        public ExtraPractice? ExtraPractice { get; set; }
        public Exercise? Exercise { get; set; }
        public Guid? ExtraPracticeChapterId { get; set; }
        public Guid? ExtraPracticeId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ExerciseId { get; set; }
        //public ICollection<ExtraPracticeExerciseAnswer> ExtraPracticeExerciseAnswers { get; set; } = new List<ExtraPracticeExerciseAnswer>();

    }
}
