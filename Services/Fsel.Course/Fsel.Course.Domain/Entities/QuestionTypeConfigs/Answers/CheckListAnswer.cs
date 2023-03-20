// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CheckListAnswer
    {
        public int Scorses { get; set; }

        public IList<CheckListAnswers>? Answers { get; set; }

        public class CheckListAnswers
        {
            public int Id { get; set; }

            public bool IsChecked { get; set; }
            public bool IsExact { get; set; }
        }
    }
}
