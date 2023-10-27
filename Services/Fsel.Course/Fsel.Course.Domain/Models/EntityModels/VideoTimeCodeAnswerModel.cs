// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeAnswerModel
    {
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
        public Guid QuestionId { get; set; }
        public Guid VideoResultId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
