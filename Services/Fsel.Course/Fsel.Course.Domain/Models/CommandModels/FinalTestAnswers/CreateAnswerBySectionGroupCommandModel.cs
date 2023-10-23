// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers
{
    public class CreateAnswerBySectionGroupCommandModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid FinalTestResultId { get; set; }
        public IList<CreateAnswerRequestModel>? Answers { get; set; }
    }

    public class CreateAnswerRequestModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
