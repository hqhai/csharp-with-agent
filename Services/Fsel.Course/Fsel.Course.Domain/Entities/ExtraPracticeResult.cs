// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ExtraPracticeResult : BaseScoreResult
    {
        [NotMapped]
        public override double PercentModule { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public Guid ExtraPracticeId { get; set; }
        public Guid? CurrentVideoTimeCodeId { get; set; }

        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; } = new List<ExtraPracticeExerciseResult>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
