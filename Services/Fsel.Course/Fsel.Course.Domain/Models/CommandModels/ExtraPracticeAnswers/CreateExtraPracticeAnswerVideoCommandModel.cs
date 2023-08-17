// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    public class CreateExtraPracticeAnswerVideoCommandModel
    {
        public Guid ExtraPracticeResultId { get; set; }
        public IList<ExtraPracticeAnswerTypeVideoModel>? Answers { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExtraPracticeAnswerTypeVideoModel
    {
        public Guid? QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
