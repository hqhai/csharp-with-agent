// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class MultipleChoiceAnswer
    {
        public IList<MultipleChoiceAnswers>? Answers { get; set; }
    }

    public class MultipleChoiceAnswers
    {
        public long Id { get; set; }

        public bool IsChecked { get; set; }

        public bool? IsExact { get; set; }
    }
}
