// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestAnswers
{
    public class CreateAnswerBySectionGroupCommandModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid MockTestResultId { get; set; }
        public Guid? StudentId { get; set; }
        public bool IsSubmit { get; set; }
        public IList<CreateAnswerRequestModel>? Answers { get; set; }
    }

    public class CreateAnswerRequestModel
    {
        public Guid? QuestionId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public object? Answer { get; set; }
    }
}
