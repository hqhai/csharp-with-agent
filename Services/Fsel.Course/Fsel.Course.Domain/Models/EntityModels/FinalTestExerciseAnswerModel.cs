// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FinalTestExerciseAnswerModel
    {
        public Guid Id { get; set; }
        public Guid FinalTestResultId { get; set; }
        public Guid ExerciseQuestionId { get; set; }
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
    }
}
