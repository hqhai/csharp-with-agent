// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTestExerciseAnswers
{
    public class CreateFinalTestExerciseAnswerCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
        public IList<FinalTestExerciseAnswerModel>? Exercises { get; set; }
    }

    public class FinalTestQuestionAnswerModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
    public class FinalTestExerciseAnswerModel
    {
        public Guid ExerciseId { get; set; }
        public IList<FinalTestQuestionAnswerModel>? Answers { get; set; }
    }
}
