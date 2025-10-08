// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class SubmitAnswerCommandModel
    {
        public Guid SectionResultId { get; set; }
        public bool IsSubmit { get; set; }

        [JsonIgnore]
        public Guid? StudentId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }
    }
}
