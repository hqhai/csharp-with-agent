// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class MutipleChoiceAnswer
    {
        public IList<MutipleChoiceAnswers>? Answers { get; set; }
    }

    public class MutipleChoiceAnswers
    {
        public long Id { get; set; }

        public bool IsChecked { get; set; }

        public bool IsExact { get; set; }
    }
}
