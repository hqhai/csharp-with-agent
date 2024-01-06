// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers
{
    using System.Text.Json.Serialization;

    public class CreateFinalTestAnswerBySectionGroupCommandModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid FinalTestResultId { get; set; }
        public bool IsSubmit { get; set; }

        [JsonIgnore]
        public Guid? StudentId { get; set; }

        public IList<CreateFinalTestAnswerRequestModel>? Answers { get; set; }
    }

    public class CreateFinalTestAnswerRequestModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
