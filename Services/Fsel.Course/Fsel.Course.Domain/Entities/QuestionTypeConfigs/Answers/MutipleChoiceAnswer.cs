// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class MutipleChoiceAnswer
    {
        public int Scores { get; set; }

        public IList<MutipleChoiceAnswers>? Answers { get; set; }
    }

    public class MutipleChoiceAnswers
    {
        public int Id { get; set; }

        public bool IsChecked { get; set; }

        public bool IsExact { get; set; }
    }
}
