// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    public class CreateExtraPracticeAnswerPlacementTestCommandModel
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
        public IList<ExtraPracticeAnswerTypePlacementTestModel>? Answers { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExtraPracticeAnswerTypePlacementTestModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
