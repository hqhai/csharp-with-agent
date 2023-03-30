// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class MultipleOptionSentenceCompletionAnswer
    {
        public IList<MultipleOptionSentenceCompletionAnswers>? Answers { get; set; }
    }

    public class MultipleOptionSentenceCompletionAnswers
    {
        public int Id { get; set; }

        public int AnswerId { get; set; }

        public bool IsExact { get; set; }
    }
}
