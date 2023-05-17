// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers
{
    public class CreatePlacementTestAnswerCommandModel
    {
        public Guid PlacementTestResultId { get; set; }
        public IList<PlacementTestAnswerQuestionModel>? Answers { get; set; }
    }

    public class PlacementTestAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
