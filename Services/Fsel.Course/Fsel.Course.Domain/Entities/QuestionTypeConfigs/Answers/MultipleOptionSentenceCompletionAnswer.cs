// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MultipleOptionSentenceCompletionAnswer
    {
        public int Scores { get; set; }

        public IList<MultipleOptionSentenceCompletionAnswers>? Answers { get; set; }
    }

    public class MultipleOptionSentenceCompletionAnswers
    {
        public int Id { get; set; }

        public int AnswerId { get; set; }

        public bool IsExact { get; set; }
    }
}
