// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class MultipleOptionSentenceCompletionQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<MultipleOptionSentenceCompletionQuestionContent>? Contents { get; set; }
    }

    public class MultipleOptionSentenceCompletionQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public IList<MultipleOptionSentenceCompletionQuestionAnswer>? Answers { get; set; }
    }

    public class MultipleOptionSentenceCompletionQuestionAnswer
    {
        public int Id { get; set; }

        [JsonRequired]
        public string? Word { get; set; }

        [JsonRequired]
        public bool? IsCorrect { get; set; }
    }
}
