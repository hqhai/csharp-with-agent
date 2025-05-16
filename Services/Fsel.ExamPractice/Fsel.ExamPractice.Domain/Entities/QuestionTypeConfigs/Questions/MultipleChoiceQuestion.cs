// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class MultipleChoiceQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<MultipleChoiceQuestionContent> Contents { get; set; } = new List<MultipleChoiceQuestionContent>();
    }

    public class MultipleChoiceQuestionContent
    {
        public long Id { get; set; }
        public string? FilePath { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public bool? IsCorrect { get; set; }
    }
}
