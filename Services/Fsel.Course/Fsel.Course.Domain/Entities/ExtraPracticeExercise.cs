// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class ExtraPracticeExercise : Entity
    {
        public ExtraPracticeChapter? ExtraPracticeChapter { get; set; }
        public ExtraPractice? ExtraPractice { get; set; }
        public Exercise? Exercise { get; set; }
        public Guid? ExtraPracticeChapterId { get; set; }
        public Guid? ExtraPracticeId { get; set; }
        public Guid? ExerciseId { get; set; }
        public ICollection<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; } = new List<ExtraPracticeExerciseResult>();

    }
}
