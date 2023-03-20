// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers.MutipleChoiceAnswer;

    public class MutipleChoiceAnswer
    {
        public int Scorses { get; set; }

        public IList<MutipleChoiceAnswers>? Answers { get; set; }

        public class MutipleChoiceAnswers
        {
            public int Id { get; set; }

            public bool IsChecked { get; set; }

            public bool IsExact { get; set; }
        }
    }
}
