// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExtraPracticeExerciseAnswerModel : BaseAnswerModel
    {
        public Guid? ExtraPracticeVideoId { get; set; }
        public Guid? ExtraPracticeExerciseId { get; set; }
        public Guid? ExerciseQuestionId { get; set; }
        public Guid StudentId { get; set; }
    }
}
