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
        public long Id { get; set; }
        public long? AnswerId { get; set; }
        public bool? IsExact { get; set; }
        public bool IsFirstSubmit { get; set; } = true;
    }
}
