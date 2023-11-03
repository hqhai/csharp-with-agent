// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers
{
    public class CreateHomeWorkAnswerCommandModel
    {
        public Guid HomeWorkResultId { get; set; }
        public bool IsSubmit { get; set; } = true;
        public IList<HomeWorkAnswerQuestionModel>? Answers { get; set; }
    }

    public class HomeWorkAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
