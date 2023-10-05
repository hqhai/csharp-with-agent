// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class ExtraPracticeResult : BaseResultScore
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public Guid ExtraPracticeId { get; set; }

        public Guid? CurrentVideoTimeCodeId { get; set; }
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; } = new List<ExtraPracticeExerciseResult>();
    }
}
