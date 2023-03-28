// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MultipleOptionSentenceCompletionQuestion
    {
        public string? Name { get; set; }

        public IList<MultipleOptionSentenceCompletionQuestionContent>? Contents { get; set; }
    }

    public class MultipleOptionSentenceCompletionQuestionContent
    {
        public int Id { get; set; }
        public string? Content { get; set; }

        public IList<MultipleOptionSentenceCompletionQuestionAnswer>? Answers { get; set; }
    }

    public class MultipleOptionSentenceCompletionQuestionAnswer
    {
        public int Id { get; set; }

        public string? Word { get; set; }

        public bool? IsCorrect { get; set; }
    }
}
