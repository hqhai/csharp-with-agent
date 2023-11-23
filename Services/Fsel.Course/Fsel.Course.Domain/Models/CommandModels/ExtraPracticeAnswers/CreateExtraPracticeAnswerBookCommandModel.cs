// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    public class CreateExtraPracticeAnswerBookCommandModel
    {
        public Guid ExtraPracticeResultId { get; set; }
        public Guid ExtraPracticeExerciseId { get; set; }
        public IList<ExtraPracticeAnswerTypeBookModel>? Answers { get; set; }
        public bool IsSubmit { get; set; }
    }

    public class ExtraPracticeAnswerTypeBookModel
    {
        public Guid? QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
