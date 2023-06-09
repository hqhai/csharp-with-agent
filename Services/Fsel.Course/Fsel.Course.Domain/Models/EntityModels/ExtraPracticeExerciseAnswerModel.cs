// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ExtraPracticeExerciseAnswerModel : BaseModel
    {
        public object? Answer { get; set; }
        public Guid ExtraPracticeVideoId { get; set; }
        public Guid ExtraPracticeExerciseId { get; set; }
        public Guid ExerciseQuestionId { get; set; }
        public Guid StudentId { get; set; }
    }
}
