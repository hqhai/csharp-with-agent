// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class FinalTestExerciseAnswerModel : BaseModel
    {
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
        public Guid FinalTestResultId { get; set; }
        public Guid ExerciseQuestionId { get; set; }
    }
}
