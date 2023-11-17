// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestAnswers
{
    using System.Text.Json.Serialization;

    public class CreateAnswerBySectionGroupCommandModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid MockTestResultId { get; set; }
        [JsonIgnore]
        public Guid? StudentId { get; set; }
        public bool IsSubmit { get; set; }
        public IList<MockTestAnswerQuestionModel>? Answers { get; set; }
    }
}
